using Lefty.Cyan.Azure;
using Lefty.Cyan.Azure.DevOps;
using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Lefty.Cyan;

/// <summary />
[Command( "plan", Description = "Generates scripts" )]
public class PlanCommand
{
    private readonly CyanConfiguration _config;
    private readonly AzService _az;
    private readonly RepositoryService _repo;
    private readonly ILogger<PlanCommand> _logger;

    /// <summary />
    public PlanCommand( IOptions<CyanConfiguration> config,
        AzService az,
        RepositoryService repo,
        ILogger<PlanCommand> logger )
    {
        _config = config.Value;
        _az = az;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    public enum PlanScope
    {
        /// <summary />
        Org = 1,

        /// <summary />
        Devops = 2,

        /// <summary />
        Azure = 4,

        /// <summary />
        All = 7,
    }


    /// <summary />
    [Argument( 0, Description = "" )]
    public PlanScope Scope { get; set; } = PlanScope.All;



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

        return 0;
    }


    /// <summary />
    private async Task PlanAzureRbac()
    {
        // TODO

        await Task.Yield();
    }


    /// <summary />
    private async Task PlanDevops()
    {
        /*
         * 
         */
        var actual = await _az.DevOpsProjectAsync( true, false, false );
        var expected = _repo.DevopsGet();


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
                sb.AppendFormat( @"az devops project create --org ""https://dev.azure.com/{0}"" --name {1} --source-control git --visibility private ", _config.DevopsOrganization, proj.Name );
                sb.AppendLine();
            }

            if ( proj.Description != null && proj.Description != m?.Description )
            {
                _logger.LogInformation( "Update project {Project} description", proj.Name );
                sb.AppendFormat( @"az devops project update --org ""https://dev.azure.com/{0}"" --project {1} --description ""{2}"" ", _config.DevopsOrganization, proj.Name, proj.Description );
                sb.AppendLine();
            }

            foreach ( var repo in proj.Repositories )
            {
                if ( m?.Repositories?.Count( x => x.Name == repo.Name ) == 1 )
                    continue;

                _logger.LogInformation( "Add repository {Project}/{Repository}", proj.Name, repo.Name );
                sb.AppendFormat( @"az repos create --org ""https://dev.azure.com/{0}"" --project {1} --name {2}", _config.DevopsOrganization, proj.Name, repo.Name );
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
            _logger.LogInformation( "Write plan-devops.ps1" );

            var xb = new StringBuilder();
            xb.AppendLine( $"{_config.DevopsOrganization} (devops organization)" );
            xb.AppendLine( $"# ---------------------------------------------------------" );
            xb.AppendLine( $"" );
            xb.AppendLine( sb.ToString() );
            xb.AppendLine( $"" );
            xb.AppendLine( $"# eof" );

            File.WriteAllText( "apply-org.ps1", xb.ToString() );
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

            if ( IsExpired( tu.DateExpiry ) == true )
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
            _logger.LogInformation( "Write plan-devops.ps1" );

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

            File.WriteAllText( "apply-devops.ps1", xb.ToString() );
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


    /// <summary />
    private bool IsExpired( DateOnly dateExpiry )
    {
        return false;
    }
}