using Lefty.Cyan.Azure;
using Lefty.Cyan.Azure.Account;
using Lefty.Cyan.Azure.DevOps;
using Lefty.Cyan.Azure.Roles;
using Lefty.Cyan.Infrastructure;
using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml;

namespace Lefty.Cyan;

/// <summary />
[Command( "plan", Description = "Generates scripts" )]
public class PlanCommand
{
    private readonly CyanConfiguration _config;
    private readonly AzService _az;
    private readonly RepositoryService _repo;
    private readonly RuleService _bre;
    private readonly ILogger<PlanCommand> _logger;

    /// <summary />
    public PlanCommand( IOptions<CyanConfiguration> config,
        AzService az,
        RepositoryService repo,
        RuleService bre,
        ILogger<PlanCommand> logger )
    {
        _config = config.Value;
        _az = az;
        _repo = repo;
        _bre = bre;
        _logger = logger;
    }


    /// <summary />
    [Flags]
    public enum PlanScope
    {
        /// <summary />
        None = 0,

        /// <summary />
        Org = 1,

        /// <summary />
        Devops = 2,

        /// <summary />
        Azure = 4,

        /// <summary />
        Jump = 8,

        /// <summary />
        Entra = 16,

        /// <summary />
        All = 31,
    }


    /// <summary />
    [Argument( 0, Description = "" )]
    public PlanScope Scope { get; set; } = PlanScope.All;

    /// <summary />
    [Option( "-O|--output-dir", CommandOptionType.SingleValue, Description = "Directory where file should be generated, otherwise PWD" )]
    public string? OutputDirectory { get; set; }



    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        /*
         * 
         */
        if ( File.Exists( Path.Combine( _config.Root, "cyan.xsd" ) ) == false )
        {
            _logger.LogError( "Invalid root, does not contain cyan.xsd file" );
            return 1;
        }


        /*
         * 
         */
        if ( this.Scope.HasFlag( PlanScope.Org ) == true )
            await PlanDevops();

        if ( this.Scope.HasFlag( PlanScope.Devops ) == true )
            await PlanDevopsRbac();

        if ( this.Scope.HasFlag( PlanScope.Azure ) == true )
            await PlanAzureRbac();

        if ( this.Scope.HasFlag( PlanScope.Entra ) == true )
            await PlanEntra();

        return 0;
    }


    /// <summary />
    private async Task PlanEntra()
    {
        var sb = new StringBuilder();
        var expected = _repo.Entra();
        var mgr = _repo.NamespaceManager();


        /*
         * 
         */
        var actual = new Dictionary<string, List<EntraMember>>();

        foreach ( var groupAttr in expected.Data.SelectNodes( " /c:entra/c:group/@name ", mgr )!.OfType<XmlAttribute>() )
        {
            var members = await _az.EntraMemberListAync( groupAttr.Value );
            actual.Add( groupAttr.Value, members );
        }


        /*
         * Add missing
         */
        var expected2 = new Dictionary<string, List<string>>();

        foreach ( var groupElem in expected.Data.SelectNodes( " /c:entra/c:group ", mgr )!.OfType<XmlElement>() )
        {
            var groupName = groupElem.Attributes[ "name" ]!.Value;
            var members = actual[ groupName ];
            expected2.Add( groupName, new List<string>() );

            foreach ( var userAttr in groupElem.SelectNodes( " c:user/@name ", mgr )!.OfType<XmlAttribute>() )
            {
                var upn = userAttr.Value + _config.EntraDomain;
                expected2[ groupName ].Add( upn.ToLowerInvariant() );
                var m = members.SingleOrDefault( x => x.UserPrincipalName.ToLowerInvariant() == upn.ToLowerInvariant() );

                if ( m != null )
                    continue;

                sb.AppendLine( $"# {groupName}: add {userAttr.Value}" );
                sb.AppendLine( $"$u = az ad user show --id {upn} | ConvertFrom-Json" );
                sb.AppendLine( $"az ad group member add --group \"{groupName}\" --member-id $u.id" );
                sb.AppendLine();
            }
        }


        /*
         * Remove unnecessary
         */
        foreach ( var group in actual )
        {
            foreach ( var member in group.Value )
            {
                if ( expected2[ group.Key ].Contains( member.UserPrincipalName.ToLowerInvariant() ) == true )
                    continue;

                sb.AppendLine( $"# {group.Key}: remove {member.UserPrincipalName}" );
                sb.AppendLine( $"az ad group member remove --group \"{group.Key}\" --member-id {member.Id}" );
                sb.AppendLine();
            }
        }


        /*
         * 
         */
        if ( sb.Length > 0 )
        {
            _logger.LogInformation( "Write apply-entra.ps1" );

            var xb = new StringBuilder();
            xb.AppendLine( $"#" );
            xb.AppendLine( $"# {_config.DevopsOrganization} (Entra)" );
            xb.AppendLine( $"# ---------------------------------------------------------" );
            xb.AppendLine( $"" );
            xb.AppendLine( sb.ToString() );
            xb.AppendLine( $"# eof" );

            var fileName = FilenameForOutput( "apply-entra.ps1" );
            File.WriteAllText( fileName, xb.ToString() );
        }
    }


    /// <summary />
    private async Task PlanAzureRbac()
    {
        /*
         * Resource identifiers
         */
        var mgr = _repo.NamespaceManager();
        var azure = _repo.Azure();
        var scopes = new List<AssignmentScope>();

        foreach ( var subElem in azure.Data.SelectNodes( " /c:azure/c:subscription ", mgr )!.OfType<XmlElement>() )
        {
            var sub = subElem.GetAttribute( "name" );
            var subId = await _az.SubscriptionGetAsync( sub );

            if ( IsAllowed( subElem ) == true )
            {
                var rt = ToRbacType( subElem );

                scopes.Add( new AssignmentScope()
                {
                    Type = subElem.LocalName,
                    Name = sub,
                    Scope = subId,
                    AllowPermanent = rt.AllowPermanent,
                    AllowEligible = rt.AllowEligible,
                } );
            }


            foreach ( var rgElem in subElem.SelectNodes( " c:resourceGroup ", mgr )!.OfType<XmlElement>() )
            {
                var rg = rgElem.GetAttribute( "name" );

                if ( IsAllowed( rgElem ) == true )
                {
                    var rt = ToRbacType( rgElem );

                    scopes.Add( new AssignmentScope()
                    {
                        Type = rgElem.LocalName,
                        Name = rg,
                        Scope = $"{subId}/resourceGroups/{rg}",
                        AllowPermanent = rt.AllowPermanent,
                        AllowEligible = rt.AllowEligible,
                    } );
                }


                foreach ( var resElem in rgElem.SelectNodes( " c:* ", mgr )!.OfType<XmlElement>() )
                {
                    var resType = resElem.LocalName;
                    var resName = resElem.GetAttribute( "name" );
                    var resProv = _az.ResourceTypeFor( resType );
                    var rt = ToRbacType( resElem );

                    scopes.Add( new AssignmentScope()
                    {
                        Type = resType,
                        Name = resName,
                        Scope = $"{subId}/resourceGroups/{rg}/providers/{resProv}/{resName}",
                        AllowPermanent = rt.AllowPermanent,
                        AllowEligible = rt.AllowEligible,
                    } );
                }
            }
        }


        /*
         * 
         */
        foreach ( var scope in scopes )
        {
            if ( scope.AllowPermanent == true )
                scope.Permanent = await _az.RoleAssignmentListAsync( scope.Scope );

            if ( scope.AllowEligible == true )
                scope.Eligible = await _az.ElligibleRoleAssignmentListAsync( scope.Scope );
        }


        /*
         * 
         */
        //File.WriteAllText( "azure.json", System.Text.Json.JsonSerializer.Serialize( scopes, new System.Text.Json.JsonSerializerOptions()
        //{
        //    WriteIndented = true,
        //} ) );


        /*
         * Permamenent / Add
         */
        var keep = new List<string>();

        var dir = _repo.DirectoryGet();
        var sb = new StringBuilder();

        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            if ( p.Username == null || p.PrincipalName == null )
                continue;

            if ( p.IsEnabled == false )
                continue;

            var rbac = _repo.PersonRbac( p.CompanyCode, p.Name ).Data;

            foreach ( var r in rbac.SelectNodes( " /c:rbac/c:azure/c:* ", mgr )!.OfType<XmlElement>() )
            {
                if ( IsRbacType( r, "permanent" ) == false )
                    continue;

                var resType = r.LocalName;
                var resName = r.GetAttribute( "name" );
                var role = r.GetAttribute( "role" );

                var s = scopes.SingleOrDefault( x => x.Type == resType && x.Name == resName );

                if ( s == null )
                {
                    _logger.LogWarning( "Resource {ResourceType}/{ResourceName} not found as scope", resType, resName );
                    continue;
                }

                var exists = s.Permanent?
                    .Where( x => x.Role == role )
                    .SingleOrDefault( x => x.PrincipalName.ToLowerInvariant() == p.PrincipalName );

                if ( exists != null )
                {
                    keep.Add( exists.Id );

                    if ( exists.Description == null || exists.Description.StartsWith( "cyan|" ) == false )
                        _logger.LogWarning( "RBAC {ResourceType}/{ResourceName} for {PrincipalName} not managed by cyan", resType, resName, p.PrincipalName );

                    continue;
                }

                sb.AppendLine( $"" );
                sb.AppendLine( $"# {p.PrincipalName} - {resType}/{resName}" );
                sb.AppendLine( $"az role assignment create --assignee {p.PrincipalName} --role \"{role}\" --scope \"{s.Scope}\" --description=\"cyan|{_config.DevopsOrganization}\" " );
            }
        }


        /*
         * Eligible / Add
         */
        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            if ( p.Username == null || p.PrincipalName == null )
                continue;

            if ( p.IsEnabled == false )
                continue;

            var rbac = _repo.PersonRbac( p.CompanyCode, p.Name ).Data;

            foreach ( var r in rbac.SelectNodes( " /c:rbac/c:azure/c:* ", mgr )!.OfType<XmlElement>() )
            {
                if ( IsRbacType( r, "eligible" ) == false )
                    continue;

                var resType = r.LocalName;
                var resName = r.GetAttribute( "name" );
                var role = r.GetAttribute( "role" );

                var s = scopes.SingleOrDefault( x => x.Type == resType && x.Name == resName );

                if ( s == null )
                {
                    _logger.LogWarning( "Resource {ResourceType}/{ResourceName} not found as scope", resType, resName );
                    continue;
                }

                var exists = s.Eligible?
                    .Where( x => x.Role == role )
                    .SingleOrDefault( x => x.PrincipalName.ToLowerInvariant() == p.PrincipalName );

                if ( exists != null )
                {
                    keep.Add( exists.Id );
                    continue;
                }

                _logger.LogWarning( "Creating eligible RBAC is not yet implemented: {ResourceType}/{ResourceName} for {PrincipalName}", resType, resName, p.PrincipalName );
            }
        }


        /*
         * Permanent / Remove
         */
        foreach ( var scope in scopes )
        {
            foreach ( var ra in scope.Permanent ?? [] )
            {
                if ( keep.Contains( ra.Id ) == true )
                    continue;

                if ( ra.Description == null || ra.Description.StartsWith( "cyan|" ) == false )
                {
                    _logger.LogWarning( "RBAC {ResourceType}/{ResourceName} for {PrincipalName} not managed by cyan", scope.Type, scope.Name, ra.PrincipalName );
                    continue;
                }

                if ( ra.Description.EndsWith( "|" + _config.DevopsOrganization ) == false )
                {
                    _logger.LogWarning( "RBAC {ResourceType}/{ResourceName} for {PrincipalName} managed under {OtherOrg}", scope.Type, scope.Name, ra.PrincipalName, ra.Description.Substring( 5 ) );
                    continue;
                }

                _logger.LogInformation( "Remove {ResourceType}/{ResourceName} for {PrincipalName}", scope.Type, scope.Name, ra.PrincipalName );
                sb.AppendLine( $"" );
                sb.AppendLine( $"# {ra.PrincipalName} - {scope.Type}/{scope.Name}" );
                sb.AppendLine( $"az role assignment delete --ids \"{ra.Id}\"" );
            }

            // TODO: No way of identifying eligible RBAC created using Cyan
        }


        /*
         * 
         */
        if ( sb.Length > 0 )
        {
            _logger.LogInformation( "Write apply-azure.ps1" );

            var xb = new StringBuilder();
            xb.AppendLine( $"#" );
            xb.AppendLine( $"# {_config.DevopsOrganization} (azure)" );
            xb.AppendLine( $"# ---------------------------------------------------------" );
            xb.AppendLine( $"" );
            xb.AppendLine( sb.ToString() );
            xb.AppendLine( $"" );
            xb.AppendLine( $"# eof" );

            var fileName = FilenameForOutput( "apply-azure.ps1" );
            File.WriteAllText( fileName, xb.ToString() );
        }
    }


    /// <summary />
    private string FilenameForOutput( string fileName )
    {
        string dirName;

        if ( this.OutputDirectory == null )
            dirName = Environment.CurrentDirectory;
        else
            dirName = Path.Combine( Environment.CurrentDirectory, this.OutputDirectory );

        Directory.CreateDirectory( dirName );
        return Path.Combine( dirName, fileName );
    }


    /// <summary />
    private bool IsRbacType( XmlElement element, string type )
    {
        var v = element.HasAttribute( "type" ) == true ? element.GetAttribute( "type" ) : "permanent";
        return v == type;
    }


    /// <summary />
    private (bool AllowPermanent, bool AllowEligible) ToRbacType( XmlElement element )
    {
        if ( element.HasAttribute( "types" ) == false )
            return (true, false);

        var v = element.GetAttribute( "types" );

        return (v.Contains( "permanent" ), v.Contains( "eligible" ));
    }


    /// <summary />
    private static bool IsAllowed( XmlElement element )
    {
        if ( element.HasAttribute( "allow" ) == false )
            return true;

        return bool.Parse( element.Attributes[ "allow" ]!.Value );
    }


    /// <summary />
    public class AssignmentScope
    {
        /// <summary />
        public required string Type { get; set; }

        /// <summary />
        public required string Name { get; set; }

        /// <summary />
        public required string Scope { get; set; }

        /// <summary />
        public required bool AllowPermanent { get; set; }

        /// <summary />
        public required bool AllowEligible { get; set; }

        /// <summary />
        public List<RoleAssignment>? Permanent { get; set; }

        /// <summary />
        public List<RoleAssignment>? Eligible { get; set; }
    }


    /// <summary />
    private async Task PlanDevops()
    {
        /*
         * 
         */
        var actual = await _az.DevOpsProjectAsync( new AzService.ProjectOptions()
        {
            WithRepositories = true,
            WithGroups = true,
        } );
        var expected = _repo.DevopsGet();

        var org = "https://dev.azure.com/" + _config.DevopsOrganization;


        /*
         * Detect differences
         */
        var sb = new StringBuilder();

        foreach ( var proj in expected.Projects )
        {
            var m = actual.SingleOrDefault( x => x.Name == proj.Name );

            if ( m == null )
            {
                _logger.LogInformation( "Add project {Project}", proj.Name );
                sb.AppendFormat( @"az devops project create --org ""{0}"" --name {1} --source-control git --visibility private ", org, proj.Name );
                sb.AppendLine();
            }

            if ( m == null || proj.Description != m.Description )
            {
                _logger.LogInformation( "Update project {Project} description", proj.Name );
                sb.AppendFormat( @"az devops project update --org ""{0}"" --project {1} --description ""{2}"" ", org, proj.Name, proj.Description );
                sb.AppendLine();
            }

            foreach ( var group in proj.Groups )
            {
                if ( m?.Groups?.Count( x => x.DisplayName == group ) == 1 )
                    continue;

                _logger.LogInformation( "Add group {Project}/{Team}", proj.Name, group );
                sb.AppendFormat( @"az devops security group create --org ""{0}"" --project {1} --name ""{2}"" ", org, proj.Name, group );
                sb.AppendLine();
            }

            foreach ( var team in proj.Teams )
            {
                if ( m?.Groups?.Count( x => x.DisplayName == team ) == 1 )
                    continue;

                _logger.LogInformation( "Add team {Project}/{Team}", proj.Name, team );
                sb.AppendFormat( @"az devops team create --org ""{0}"" --project {1} --name ""{2}"" ", org, proj.Name, team );
                sb.AppendLine();
            }

            foreach ( var repo in proj.Repositories )
            {
                if ( m?.Repositories?.Count( x => x.Name == repo.Name ) == 1 )
                    continue;

                // Default repo will get created automatically
                if ( repo.Name == proj.Name && m == null )
                    continue;

                _logger.LogInformation( "Add repository {Project}/{Repository}", proj.Name, repo.Name );
                sb.AppendFormat( @"az repos create --org ""{0}"" --project {1} --name {2}", org, proj.Name, repo.Name );
                sb.AppendLine();
            }
        }


        /*
         * 
         */
        foreach ( var proj in actual )
        {
            var m = expected.Projects.SingleOrDefault( x => x.Name == proj.Name );

            if ( m == null )
                _logger.LogWarning( "Project {Project} exists in org, but not defined", proj.Name );

            if ( proj.Repositories == null )
                continue;

            foreach ( var repo in proj.Repositories )
            {
                var m2 = m?.Repositories.SingleOrDefault( x => x.Name == repo.Name );

                if ( m2 == null )
                    _logger.LogWarning( "Repository {Project}/{Repository} exists in org, but not defined", proj.Name, repo.Name );
            }
        }


        /*
         * 
         */
        if ( sb.Length > 0 )
        {
            _logger.LogInformation( "Write apply-org.ps1" );

            var xb = new StringBuilder();
            xb.AppendLine( $"#" );
            xb.AppendLine( $"# {_config.DevopsOrganization} (devops organization)" );
            xb.AppendLine( $"# ---------------------------------------------------------" );
            xb.AppendLine( $"" );
            xb.AppendLine( sb.ToString() );
            xb.AppendLine( $"" );
            xb.AppendLine( $"# eof" );

            var fileName = FilenameForOutput( "apply-org.ps1" );
            File.WriteAllText( fileName, xb.ToString() );
        }
    }


    /// <summary />
    private async Task PlanDevopsRbac()
    {
        /*
         * Actual
         */
        var org = await _az.MembershipAsync();
        //File.WriteAllText( "temporary.json", org.xJson() );
        //var json = File.ReadAllText( "temporary.json" );
        //var org = JsonSerializer.Deserialize<Organization>( json )!;


        /*
         * Expected
         */
        var dir = _repo.DirectoryGet();

        var users = dir
            .Where( x => x.Persons != null )
            .SelectMany( x => x.Persons! )
            .ToList();


        /*
         * Difference engine
         */
        var sb = new StringBuilder();
        var mgr = _repo.NamespaceManager();


        /*
         * New/existing users
         */
        foreach ( var tu in users )
        {
            if ( tu.Username == null )
            {
                _logger.LogInformation( "{CompanyCode}/{Name}: Has no username defined, skip", tu.CompanyCode, tu.Name );
                continue;
            }

            if ( tu.IsEnabled == false )
            {
                _logger.LogInformation( "{CompanyCode}/{Name}: Is disabed, skip", tu.CompanyCode, tu.Name );
                continue;
            }

            if ( _bre.IsExpired( tu.DateExpiry ) == true )
            {
                _logger.LogInformation( "{CompanyCode}/{Name}: Is expired, skip", tu.CompanyCode, tu.Name );
                continue;
            }


            /*
             * 
             */
            var rbac = _repo.PersonRbac( tu.CompanyCode, tu.Name ).Data;
            var toAccess = Enum.Parse<DevOpsAccessType>( rbac.SelectSingleNode( " /c:rbac/c:devops/@access ", mgr )!.Value! );


            /*
             * 
             */
            var hasChanges = false;
            var usb = new StringBuilder();

            usb.AppendLine( $"" );
            usb.AppendLine( $"#" );
            usb.AppendLine( $"# {tu.Name} @ {tu.CompanyCode}" );
            usb.AppendLine( $"# -------------------------------------------------------------------" );
            usb.AppendLine( $"$upn = \"{tu.PrincipalName}\"" );
            usb.AppendLine( $"" );


            /*
             * 
             */
            var u = org.Members.SingleOrDefault( x => x.User.PrincipalName.ToLowerInvariant() == tu.PrincipalName );

            if ( u == null )
            {
                usb.AppendLine( $"$r1 = az devops user add    --email-id $upn --license-type {Map( toAccess )} --org $org | ConvertFrom-Json" );
                hasChanges = true;
            }
            else if ( toAccess != u.AccessLevel.AccountLicenseType )
            {
                hasChanges = true;
                usb.AppendLine( $"$r1 = az devops user update --user $upn --license-type {Map( toAccess )} --org $org | ConvertFrom-Json" );
            }
            else
                usb.AppendLine( $"$r1 = az devops user show   --user $upn --org $org | ConvertFrom-Json" );

            usb.AppendLine( $"$ud = $r1.user.descriptor" );
            usb.AppendLine();


            /*
             * Add to groups/teams
             */
            foreach ( var toProject in rbac.SelectNodes( " /c:rbac/c:devops/c:project ", mgr )!.OfType<XmlElement>() )
            {
                var pname = toProject.Attributes[ "name" ]!.Value;

                foreach ( var toGroup in toProject.SelectNodes( " c:group ", mgr )!.OfType<XmlElement>() )
                {
                    var gname = toGroup.Attributes[ "name" ]!.Value;
                    var p = org.Projects.SingleOrDefault( x => x.Name == pname );

                    if ( p == null )
                    {
                        _logger.LogError( "{CompanyCode}/{Name}: has RBAC for {Project} which does not exist", tu.CompanyCode, tu.Name, pname );
                        continue;
                    }

                    var g = p.Groups?.SingleOrDefault( x => x.DisplayName == gname );

                    if ( g == null )
                    {
                        _logger.LogError( "{CompanyCode}/{Name}: has RBAC for {Project}/{Group} which does not exist", tu.CompanyCode, tu.Name, pname, gname );
                        continue;
                    }

                    var addGroup = false;

                    if ( u == null )
                        addGroup = true;
                    else if ( g.Members?.SingleOrDefault( x => x == tu.PrincipalName!.ToLowerInvariant() ) == null )
                        addGroup = true;

                    if ( addGroup == false )
                        continue;

                    usb.AppendLine( $"# {pname}/{gname}" );
                    usb.AppendLine( $"az devops security group membership add --group-id {g.Descriptor} --member-id $ud" );
                    hasChanges = true;
                }
            }


            /*
             * Remove from groups/teams
             */
            foreach ( var fromProject in org.Projects )
            {
                foreach ( var fromGroup in fromProject.Groups ?? [] )
                {
                    if ( fromGroup.Members?.SingleOrDefault( x => x == tu.PrincipalName!.ToLowerInvariant() ) == null )
                        continue;

                    if ( rbac.SelectSingleNode( $" /c:rbac/c:devops/c:project[ @name = '{fromProject.Name}' ]/c:group[ @name = '{fromGroup.DisplayName}' ] ", mgr ) != null )
                        continue;

                    usb.AppendLine( $"# group: {fromProject.Name}/{fromGroup.DisplayName}" );
                    usb.AppendLine( $"az devops security group membership remove --group-id {fromGroup.Descriptor} --member-id $ud --yes" );
                    hasChanges = true;
                }

                foreach ( var fromTeam in fromProject.Teams ?? [] )
                {
                    if ( fromTeam.Members?.SingleOrDefault( x => x == tu.PrincipalName!.ToLowerInvariant() ) == null )
                        continue;

                    if ( rbac.SelectSingleNode( $" /c:rbac/c:devops/c:project[ @name = '{fromProject.Name}' ]/c:team[ @name = '{fromTeam.DisplayName}' ] ", mgr ) != null )
                        continue;

                    usb.AppendLine( $"# team: {fromProject.Name}/{fromTeam.DisplayName}" );
                    usb.AppendLine( $"az devops security group membership remove --group-id {fromTeam.Descriptor} --member-id $ud --yes" );
                    hasChanges = true;
                }
            }


            /*
             * 
             */
            if ( hasChanges == false )
                continue;

            sb.Append( usb.ToString() );
        }


        /*
         * Remove users
         */
        foreach ( var fromMember in org.Members )
        {
            var tu = users
                .Where( x => x.PrincipalName != null )
                .SingleOrDefault( x => x.PrincipalName!.ToLowerInvariant() == fromMember.User.PrincipalName.ToLowerInvariant() );

            if ( tu != null )
                continue;

            sb.AppendLine( $"#" );
            sb.AppendLine( $"# {fromMember.User.PrincipalName}" );
            sb.AppendLine( $"# -------------------------------------------------------------------" );
            sb.AppendLine( $"$upn = \"{fromMember.User.PrincipalName}\"" );
            sb.AppendLine( $"" );
            sb.AppendLine( $"az devops user remove --user $upn --org $org --yes" );
            sb.AppendLine( $"" );
        }


        /*
         * 
         */
        if ( sb.Length > 0 )
        {
            _logger.LogInformation( "Write apply-devops.ps1" );

            var xb = new StringBuilder();
            xb.AppendLine( $"# " );
            xb.AppendLine( $"# {_config.DevopsOrganization} (devops RBAC)" );
            xb.AppendLine( $"# ---------------------------------------------------------" );
            xb.AppendLine( $"$org = \"https://dev.azure.com/{_config.DevopsOrganization}\"" );
            xb.AppendLine( $"az devops configure -d organization=$org" );
            xb.AppendLine( $"" );
            xb.AppendLine( sb.ToString() );
            xb.AppendLine( $"" );
            xb.AppendLine( $"# eof" );

            var fileName = FilenameForOutput( "apply-devops.ps1" );
            File.WriteAllText( fileName, xb.ToString() );
        }
    }


    /// <summary />
    private static string Map( DevOpsAccessType value )
    {
        return value switch
        {
            DevOpsAccessType.Basic => "express",
            DevOpsAccessType.BasicAndTest => "advanced",
            DevOpsAccessType.Stakeholder => "stakeholder",

            _ => throw new NotImplementedException()
        };
    }
}