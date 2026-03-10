using Lefty.Cyan.Repository.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Cyan.Repository;

/// <summary />
public partial class RepositoryService
{
    private readonly CyanConfiguration _config;
    private readonly XmlNamespaceManager _mgr;
    private readonly ILogger<RepositoryService> _logger;


    /// <summary />
    public RepositoryService( IOptions<CyanConfiguration> config,
        ILogger<RepositoryService> logger )
    {
        _config = config.Value;
        _logger = logger;

        var mgr = new XmlNamespaceManager( new NameTable() );
        mgr.AddNamespace( "c", "urn:cyan" );

        _mgr = mgr;
    }


    /// <summary />
    public XmlNamespaceManager NamespaceManager()
    {
        return _mgr;
    }


    /// <summary />
    public string SkeletonGet( string skel )
    {
        var rest = skel.Replace( "/", "." );

        using var resx = typeof( RepositoryService ).Assembly.GetManifestResourceStream( $"Lefty.Cyan.Repository.Resources.{rest}" );
        using var sr = new StreamReader( resx! );

        return sr.ReadToEnd();
    }


    /// <summary />
    public string SchemaGet()
    {
        using var resx = typeof( RepositoryService ).Assembly.GetManifestResourceStream( "Lefty.Cyan.Repository.Resources.cyan.xsd" );
        using var sr = new StreamReader( resx! );

        return sr.ReadToEnd();
    }


    /// <summary />
    private XmlSchemaSet SchemaSetGet()
    {
        using var resx = typeof( RepositoryService ).Assembly.GetManifestResourceStream( "Lefty.Cyan.Repository.Resources.cyan.xsd" );
        using var reader = XmlReader.Create( resx! );

        var xsd = XmlSchema.Read( reader, null )!;

        var ss = new XmlSchemaSet( new NameTable() );
        ss.Add( xsd );

        return ss;
    }


    /// <summary />
    private Result<XmlDocument> Validate( string rootElement, string file )
    {
        var path = Path.Combine( _config.Root, file );
        _logger.LogDebug( "{File}: Validate", file );

        if ( File.Exists( path ) == false )
        {
            _logger.LogError( "{File}: Required file is missing", file );
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
            _logger.LogError( ex, "{File}: Failed to load XML", file );
            return new Result<XmlDocument>( "E002", $"File {file} failed to load" );
        }


        /*
         * Validate against schema
         */
        xml.Schemas = SchemaSetGet();

        var errors = new List<string>();

        xml.Validate( ( sender, e ) =>
        {
            errors.Add( e.Message );
        } );

        if ( errors.Count > 0 )
        {
            foreach ( var error in errors )
                _logger.LogError( "{File}: xsd: {Error}", file, error );

            return new Result<XmlDocument>( "E003", $"File {file} failed schema validation" );
        }


        /*
         * Check root element
         */
        if ( xml.DocumentElement == null )
        {
            _logger.LogError( "{File}: Xml has no document element", file );
            return new Result<XmlDocument>( "E004", $"Xml {file} has no document element" );
        }

        if ( xml.DocumentElement.NamespaceURI != "urn:cyan" )
        {
            _logger.LogError( "{File}: Xml has invalid namespace {Actual}, expected {Expected}",
                file, xml.DocumentElement.NamespaceURI, "urn:cyan" );

            return new Result<XmlDocument>( "E005", $"Xml {file} has invalid namespace" );
        }

        if ( xml.DocumentElement?.LocalName != rootElement )
        {
            _logger.LogError( "{File}: File has invalid root {Actual}, expected {Expected}",
                file, xml.DocumentElement?.LocalName, rootElement );

            return new Result<XmlDocument>( "E006", $"File {file} has incorrect root element" );
        }

        return new Result<XmlDocument>( xml );
    }


    /// <summary />
    public Company ToCompany( string companyCode, XmlDocument xml )
    {
        var ns = NamespaceManager();
        var obj = new Company()
        {
            Code = companyCode,
            Name = xml.SelectSingleNode( " /c:company/c:name ", ns )!.InnerText,
            Description = xml.SelectSingleNode( " /c:company/c:description ", ns )!.InnerText,
        };

        return obj;
    }


    /// <summary />
    public Person ToPerson( string companyCode, XmlDocument xml )
    {
        var ns = NamespaceManager();
        var p = new Person()
        {
            CompanyCode = companyCode,
            Id = xml.SelectSingleNode( " /c:person/c:id ", ns )?.InnerText,
            Username = xml.SelectSingleNode( " /c:person/c:username ", ns )?.InnerText,
            Name = xml.SelectSingleNode( " /c:person/c:name ", ns )!.InnerText,
            Expires = DateOnly.ParseExact( xml.SelectSingleNode( " /c:person/c:expires ", ns )!.InnerText, "yyyy-MM-dd" ),
            IsEnabled = bool.Parse( xml.SelectSingleNode( " /c:person/c:enabled ", ns )?.InnerText ?? "true" ),

            Email = xml.SelectSingleNode( " /c:person/c:email ", ns )?.InnerText,
            Phone = xml.SelectSingleNode( " /c:person/c:phone ", ns )?.InnerText,
            Role = xml.SelectSingleNode( " /c:person/c:role ", ns )?.InnerText,
        };

        if ( p.Username != null )
        {
            if ( p.Username?.Contains( "@" ) == false )
                p.PrincipalName = p.Username + _config.EntraDomain;
            else
                p.PrincipalName = p.Username;
        }

        return p;
    }
}