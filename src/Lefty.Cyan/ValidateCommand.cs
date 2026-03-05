using Lefty.Cyan.Model;
using Lefty.Cyan.Services;
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
    private readonly RepositoryService _svc;
    private readonly ILogger<ValidateCommand> _logger;


    /// <summary />
    public ValidateCommand( IOptions<CyanConfiguration> config,
        RepositoryService svc,
        ILogger<ValidateCommand> logger )
    {
        _config = config.Value;
        _svc = svc;
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
        var azure = add( Validate( "azure", "system/azure.xml" ) );
        var devops = add( Validate( "devops", "system/devops.xml" ) );
        var dns = add( Validate( "dns", "system/dns.xml" ) );
        var jump = add( Validate( "jump", "system/jump.xml" ) );


        /*
         * 
         */
        var dir = new DirectoryInfo( Path.Combine( _config.Root, "people" ) );
        var people = new List<Person>();

        foreach ( var corpDir in dir.GetDirectories() )
        {
            var corp = add( Validate( "company", $"people/{corpDir.Name}/index.xml" ) );

            foreach ( var personDir in corpDir.GetDirectories() )
            {
                // Person
                var pfile = $"people/{corpDir.Name}/{personDir.Name}/index.xml";
                var pxml = add( Validate( "person", pfile ) );

                if ( pxml != null )
                {
                    var p = _svc.Person( corpDir.Name, pxml );
                    people.Add( p );

                    add2( ValidatePerson( pfile, personDir.Name, p ) );
                }

                // Standing RBAC
                var mainf = $"people/{corpDir.Name}/{personDir.Name}/rbac.xml";
                var main = add( Validate( "rbac", mainf ) );

                if ( main != null )
                {
                    add2( ValidateMainRbac( mainf, main ) );
                    add2( ValidateAzureRbac( mainf, main, azure ) );
                    add2( ValidateDevopsRbac( mainf, main, devops ) );
                    add2( ValidateJumpRbac( mainf, main, jump ) );
                }

                // Temporary RBAC
                foreach ( var x in personDir.GetFiles( "rbac-*.xml" ) )
                {
                    var tempf = $"people/{corpDir.Name}/{personDir.Name}/{x.Name}";
                    var temp = add( Validate( "rbac", tempf ) );

                    if ( temp == null )
                        continue;

                    add2( ValidateTemporaryRbac( tempf, temp ) );
                    add2( ValidateAzureRbac( tempf, temp, azure ) );
                    add2( ValidateDevopsRbac( tempf, temp, devops ) );
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

        if ( p.Name != name )
        {
            _logger.LogError( "Person {File} has name {Actual}, expected {Expected}", pfile, p.Name, name );
            isOk = false;
        }

        if ( isOk == false )
            return new Result<bool>( "J001", "Main RBAC must not have window" );

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateMainRbac( string file, XmlDocument rbac )
    {
        var mgr = _svc.NamespaceManager();
        var node = rbac.SelectSingleNode( " /c:rbac/c:window ", mgr )!;

        if ( node != null )
        {
            _logger.LogError( "{File} has <window />, which is not allowed for standing RBAC", file );
            return new Result<bool>( "G001", "Main RBAC must not have window" );
        }

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateTemporaryRbac( string file, XmlDocument rbac )
    {
        var mgr = _svc.NamespaceManager();
        var node = (XmlElement) rbac.SelectSingleNode( " /c:rbac/c:window ", mgr )!;

        if ( node == null )
        {
            _logger.LogError( "{File} is missing <window />, which is required for temporary RBAC", file );
            return new Result<bool>( "G002", "Temporary RBAC must have window" );
        }


        /*
         * 
         */
        var from = DateOnly.ParseExact( node.Attributes[ "from" ]!.Value, "yyyy-MM-dd" );
        var to = DateOnly.ParseExact( node.Attributes[ "to" ]!.Value, "yyyy-MM-dd" );

        if ( from > to )
        {
            _logger.LogError( "{File} has invalid <window />: @from is after @to", file );
            return new Result<bool>( "G003", "Invalid window range" );
        }

        return new Result<bool>( true );
    }


    /// <summary />
    private Result<bool> ValidateAzureRbac( string file, XmlDocument rbac, XmlDocument? azure )
    {
        if ( azure == null )
            return new Result<bool>( true );

        var mgr = _svc.NamespaceManager();
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

            var mn = azure.SelectSingleNode( $" /c:azure/c:{ln}[ @name = '{rn}' ] ", mgr );

            if ( mn != null )
                continue;

            ok = false;
            _logger.LogError( "{File} grants Azure RBAC to {ResourceType}/{ResourceName} which is not defined", file, ln, rn );
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

        var mgr = _svc.NamespaceManager();
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
    private Result<bool> ValidateJumpRbac( string file, XmlDocument rbac, XmlDocument? jump )
    {
        if ( jump == null )
            return new Result<bool>( true );

        var mgr = _svc.NamespaceManager();
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


    /// <summary />
    private Result<XmlDocument> Validate( string rootElement, string file )
    {
        var path = Path.Combine( _config.Root, file );
        _logger.LogDebug( "Check {File}", file );

        if ( File.Exists( path ) == false )
        {
            _logger.LogError( "File {File} is missing", file );
            return new Result<XmlDocument>( "E001", $"File {file} is missing" );
        }


        /*
         * 
         */
        var xml = new XmlDocument();

        try
        {
            xml.Load( path );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to load XML {File}", file );
            return new Result<XmlDocument>( "E002", $"File {file} failed to load" );
        }


        /*
         * Validate against schema
         */
        xml.Schemas = _svc.SchemaSetGet();

        var errors = new List<string>();

        xml.Validate( ( sender, e ) =>
        {
            errors.Add( e.Message );
        } );

        if ( errors.Count > 0 )
        {
            foreach ( var error in errors )
                _logger.LogError( "{File} xsd: {Error}", file, error );

            return new Result<XmlDocument>( "E003", $"File {file} failed schema validation" );
        }


        /*
         * Check root element
         */
        if ( xml.DocumentElement == null )
        {
            _logger.LogError( "Xml {File} has no document element", file );
            return new Result<XmlDocument>( "E004", $"Xml {file} has no document element" );
        }

        if ( xml.DocumentElement.NamespaceURI != "urn:cyan" )
        {
            _logger.LogError( "Xml {File} has invalid namespace {Actual}, expected {Expected}",
                file, xml.DocumentElement.NamespaceURI, "urn:cyan" );

            return new Result<XmlDocument>( "E005", $"Xml {file} has invalid namespace" );
        }

        if ( xml.DocumentElement?.LocalName != rootElement )
        {
            _logger.LogError( "File {File} has invalid root {Actual}, expected {Expected}",
                file, xml.DocumentElement?.LocalName, rootElement );

            return new Result<XmlDocument>( "E006", $"File {file} has incorrect root element" );
        }

        return new Result<XmlDocument>( xml );
    }
}