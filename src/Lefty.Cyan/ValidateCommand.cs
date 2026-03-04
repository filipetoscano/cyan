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
        var res = new List<bool>();
        var add = ( Result<XmlDocument?> r ) =>
        {
            res.Add( r.IsOk );

            if ( r.IsOk == false )
                return null;

            return r.Data;
        };


        /*
         * 
         */
        var azure = add( Validate( "azure", "system/azure.xml" ) );
        var devops = add( Validate( "devops", "system/devops.xml" ) );
        var dns = add( Validate( "dns", "system/dns.xml" ) );


        /*
         * 
         */
        var dir = new DirectoryInfo( Path.Combine( _config.Root, "people" ) );

        foreach ( var corpDir in dir.GetDirectories() )
        {
            var corp = add( Validate( "company", $"people/{corpDir.Name}/index.xml", true ) );

            foreach ( var personDir in corpDir.GetDirectories() )
            {
                add( Validate( "person", $"people/{corpDir.Name}/{personDir.Name}/index.xml" ) );

                foreach ( var x in personDir.GetFiles( "rbac*.xml" ) )
                    add( Validate( "rbac", $"people/{corpDir.Name}/{personDir.Name}/{x.Name}" ) );
            }
        }


        /*
         * 
         */
        if ( res.Any( x => x == false ) == true )
            return 1;

        return 0;
    }


    /// <summary />
    private Result<XmlDocument?> Validate( string rootElement, string file, bool required = false )
    {
        XmlDocument? xml = null;


        /*
         * 
         */
        var path = Path.Combine( _config.Root, file );
        _logger.LogDebug( "Check {File}", file );

        if ( File.Exists( path ) == false )
        {
            if ( required == true )
            {
                _logger.LogError( "File {File} is missing", file );
                return new Result<XmlDocument?>( "E001", $"File {file} is missing" );
            }

            _logger.LogWarning( "File {File} not found, skipping...", file );
            return new Result<XmlDocument?>( xml );
        }


        /*
         * 
         */
        xml = new XmlDocument();

        try
        {
            xml.Load( path );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to load XML {File}", file );
            return new Result<XmlDocument?>( "E002", $"File {file} failed to load" );
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

            return new Result<XmlDocument?>( "E003", $"File {file} failed schema validation" );
        }


        /*
         * Check root element
         */
        if ( xml.DocumentElement == null )
        {
            _logger.LogError( "Xml {File} has no document element", file );
            return new Result<XmlDocument?>( "E004", $"Xml {file} has no document element" );
        }

        if ( xml.DocumentElement.NamespaceURI != "urn:cyan" )
        {
            _logger.LogError( "Xml {File} has invalid namespace {Actual}, expected {Expected}",
                file, xml.DocumentElement.NamespaceURI, "urn:cyan" );

            return new Result<XmlDocument?>( "E005", $"Xml {file} has invalid namespace" );
        }

        if ( xml.DocumentElement?.LocalName != rootElement )
        {
            _logger.LogError( "File {File} has invalid root {Actual}, expected {Expected}",
                file, xml.DocumentElement?.LocalName, rootElement );

            return new Result<XmlDocument?>( "E006", $"File {file} has incorrect root element" );
        }

        return new Result<XmlDocument?>( xml );
    }
}