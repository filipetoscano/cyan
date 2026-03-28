using Lefty.Cyan.Repository;
using Lefty.Cyan.Repository.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml;

namespace Lefty.Cyan;

/// <summary />
[Command( "validate", Description = "Validate repository objects" )]
public class ValidateCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _repo;
    private readonly ILogger<ValidateCommand> _logger;


    /// <summary />
    public ValidateCommand( IOptions<CyanConfiguration> config,
        RepositoryService svc,
        ILogger<ValidateCommand> logger )
    {
        _config = config.Value;
        _repo = svc;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        var isOk = true;

        var add = ( Result<XmlDocument> r ) =>
        {
            if ( r.IsOk == false )
            {
                isOk = false;
                return null;
            }

            return r.Data;
        };

        var add2 = ( Result<bool> r ) =>
        {
            if ( r.IsOk == false )
            {
                isOk = false;
                return false;
            }

            return r.Data;
        };

        var add3 = ( Result<RbacWindow> r ) =>
        {
            if ( r.IsOk == false )
            {
                isOk = false;
                return null;
            }

            return r.Data;
        };


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
        var azure = add( _repo.Azure() );
        var devops = add( _repo.Devops() );
        var dns = add( _repo.Dns() );
        var entra = add( _repo.Entra() );
        var firewall = add( _repo.Firewall() );
        var jump = add( _repo.Jump() );


        /*
         * 
         */
        var dir = new DirectoryInfo( Path.Combine( _config.Root, "people" ) );
        var people = new List<Person>();

        foreach ( var corpDir in dir.GetDirectories() )
        {
            var corp = add( _repo.Company( corpDir.Name ) );

            foreach ( var personDir in corpDir.GetDirectories() )
            {
                // Person
                var pfile = $"people/{corpDir.Name}/{personDir.Name}/index.xml";
                var pxml = add( _repo.Person( corpDir.Name, personDir.Name ) );

                if ( pxml != null )
                {
                    var p = _repo.ToPerson( corpDir.Name, pxml );
                    people.Add( p );

                    add2( ValidatePerson( pfile, personDir.Name, p ) );
                }

                // Standing RBAC
                var mainf = $"people/{corpDir.Name}/{personDir.Name}/rbac.xml";
                var main = add( _repo.PersonRbac( corpDir.Name, personDir.Name ) );

                if ( main != null )
                {
                    add2( ValidateMainRbac( mainf, main ) );
                    add2( ValidateAzureRbac( mainf, main, azure ) );
                    add2( ValidateDevopsRbac( mainf, main, devops ) );
                    add2( ValidateEntraRbac( mainf, main, entra ) );
                    add2( ValidateJumpRbac( mainf, main, jump ) );
                }

                // Temporary RBAC
                foreach ( var x in personDir.GetFiles( "rbac-*.xml" ) )
                {
                    var tempf = $"people/{corpDir.Name}/{personDir.Name}/{x.Name}";
                    var temp = add( _repo.PersonRbac( corpDir.Name, personDir.Name, Path.GetFileNameWithoutExtension( x.Name ) ) );

                    if ( temp == null )
                        continue;

                    var w = add3( ValidateTemporaryRbac( tempf, temp ) );

                    if ( w.IsPast == true )
                    {
                        _logger.LogDebug( "{File}: Skip cross-checks for past windows", tempf );
                        continue;
                    }

                    add2( ValidateAzureRbac( tempf, temp, azure ) );
                    add2( ValidateDevopsRbac( tempf, temp, devops ) );
                    add2( ValidateEntraRbac( tempf, temp, entra ) );
                    add2( ValidateJumpRbac( tempf, temp, jump ) );
                }
            }
        }


        /*
         * 
         */
        var duplicates = people
            .Where( p => p.Username is not null )
            .GroupBy( p => p.Username )
            .Where( g => g.Count() > 1 )
            .Select( g => g.Key )
            .ToList();

        foreach ( var dupe in duplicates )
        {
            _logger.LogError( "Username {Username} is set for more than one person", dupe );

            foreach ( var d in people.Where( x => x.Username == dupe ) )
                _logger.LogError( "see: {CompanyCode}/{Name}", d.CompanyCode, d.Name );
        }


        /*
         * 
         */
        if ( isOk == false )
            return 1;

        return 0;
    }


    /// <summary />
    private Result<bool> ValidatePerson( string pfile, string name, Person p )
    {
        var isOk = true;

        if ( isOk == false )
            return new Result<bool>( "J001", "Person is invalid" );

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateMainRbac( string file, XmlDocument rbac )
    {
        var mgr = _repo.NamespaceManager();
        var node = rbac.SelectSingleNode( " /c:rbac/c:window ", mgr )!;

        if ( node != null )
        {
            _logger.LogError( "{File} has <window />, which is not allowed for standing RBAC", file );
            return new Result<bool>( "G001", "Main RBAC must not have window" );
        }

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<RbacWindow> ValidateTemporaryRbac( string file, XmlDocument rbac )
    {
        var mgr = _repo.NamespaceManager();
        var node = (XmlElement) rbac.SelectSingleNode( " /c:rbac/c:window ", mgr )!;

        if ( node == null )
        {
            _logger.LogError( "{File} is missing <window />, which is required for temporary RBAC", file );
            return new Result<RbacWindow>( "G002", "Temporary RBAC must have window" );
        }


        /*
         * 
         */
        var from = DateOnly.ParseExact( node.Attributes[ "from" ]!.Value, "yyyy-MM-dd" );
        var to = DateOnly.ParseExact( node.Attributes[ "to" ]!.Value, "yyyy-MM-dd" );

        if ( from > to )
        {
            _logger.LogError( "{File} has invalid <window />: @from is after @to", file );
            return new Result<RbacWindow>( "G003", "Invalid window range" );
        }

        return new Result<RbacWindow>( new RbacWindow()
        {
            From = from,
            To = to,
        } );
    }


    /// <summary />
    private Result<bool> ValidateAzureRbac( string file, XmlDocument rbac, XmlDocument? azure )
    {
        if ( azure == null )
            return new Result<bool>( true );

        var mgr = _repo.NamespaceManager();
        var nodes = rbac.SelectNodes( " /c:rbac/c:azure/c:* ", mgr )!;

        if ( nodes.Count == 0 )
            return new Result<bool>( true );


        /*
         * 
         */
        var ok = true;

        foreach ( XmlElement n in nodes )
        {
            var ln = n.LocalName;
            var rn = n.Attributes[ "name" ]!.Value;
            var ty = n.HasAttribute( "type" ) == true ? n.GetAttribute( "type" ) : "permanent";

            // Match?
            var mn = (XmlElement) azure.SelectSingleNode( $" /c:azure//c:{ln}[ @name = '{rn}' ] ", mgr )!;

            if ( mn == null )
            {
                ok = false;
                _logger.LogError( "{File} grants Azure RBAC to {ResourceType}/{ResourceName} which is not defined", file, ln, rn );
                continue;
            }

            // Allow?
            var allow = bool.Parse( mn.HasAttribute( "allow" ) == true ? mn.GetAttribute( "allow" ) : "true" );

            if ( allow == false )
            {
                ok = false;
                _logger.LogError( "{File} grants Azure RBAC to {ResourceType}/{ResourceName} which has @allow = false", file, ln, rn );
                continue;
            }

            // Type?
            var type = mn.HasAttribute( "types" ) == true ? mn.GetAttribute( "types" ) : "permanent";

            if ( type.Contains( ty ) == false )
            {
                ok = false;
                _logger.LogError( "{File} grants Azure RBAC to {ResourceType}/{ResourceName} but type not allowed: {Requested} requested", file, ln, rn, ty );
                continue;
            }
        }


        /*
         * 
         */
        if ( ok == false )
            return new Result<bool>( "F001", "Some Azure resources failed to match" );

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateDevopsRbac( string file, XmlDocument rbac, XmlDocument? devops )
    {
        if ( devops == null )
            return new Result<bool>( true );

        var mgr = _repo.NamespaceManager();
        var nodes = rbac.SelectNodes( " /c:rbac/c:devops/c:project ", mgr )!;

        if ( nodes.Count == 0 )
            return new Result<bool>( true );


        /*
         * 
         */
        var ok = true;

        foreach ( XmlElement n in nodes )
        {
            var pn = n.Attributes[ "name" ]!.Value;
            var mn = devops.SelectSingleNode( $" /c:devops/c:project[ @name = '{pn}' ] ", mgr );

            if ( mn != null )
                continue;

            ok = false;
            _logger.LogError( "{File} grants DevOps RBAC to project {ProjectName} which is not defined", file, pn );
        }


        /*
         * 
         */
        if ( ok == false )
            return new Result<bool>( "F001", "Some DevOps projects failed to match" );

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateEntraRbac( string file, XmlDocument rbac, XmlDocument? entra )
    {
        if ( entra == null )
            return new Result<bool>( true );

        var mgr = _repo.NamespaceManager();
        var nodes = rbac.SelectNodes( " /c:rbac/c:entra/c:group ", mgr )!;

        if ( nodes.Count == 0 )
            return new Result<bool>( true );


        /*
         * 
         */
        var ok = true;

        foreach ( XmlElement n in nodes )
        {
            var gn = n.Attributes[ "name" ]!.Value;
            var mn = entra.SelectSingleNode( $" /c:entra/c:group[ @name = '{gn}' ] ", mgr );

            if ( mn != null )
                continue;

            ok = false;
            _logger.LogError( "{File} grants access to group {GroupName} which is not defined", file, gn );
        }


        /*
         * 
         */
        if ( ok == false )
            return new Result<bool>( "J001", "Some groups failed to match" );

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateJumpRbac( string file, XmlDocument rbac, XmlDocument? jump )
    {
        if ( jump == null )
            return new Result<bool>( true );

        var mgr = _repo.NamespaceManager();
        var nodes = rbac.SelectNodes( " /c:rbac/c:jump ", mgr )!;

        if ( nodes.Count == 0 )
            return new Result<bool>( true );


        /*
         * 
         */
        var ok = true;

        foreach ( XmlElement n in nodes )
        {
            var sn = n.Attributes[ "server" ]!.Value;
            var mn = jump.SelectSingleNode( $" /c:jump/c:jump[ @server = '{sn}' ] ", mgr );

            if ( mn != null )
                continue;

            ok = false;
            _logger.LogError( "{File} grants access to jump server {ServerName} which is not defined", file, sn );
        }


        /*
         * 
         */
        if ( ok == false )
            return new Result<bool>( "J001", "Some jump servers failed to match" );

        return new Result<bool>( true );
    }
}