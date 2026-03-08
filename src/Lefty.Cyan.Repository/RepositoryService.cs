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
    public XmlSchemaSet SchemaSetGet()
    {
        using var resx = typeof( RepositoryService ).Assembly.GetManifestResourceStream( "Lefty.Cyan.Repository.Resources.cyan.xsd" );
        using var reader = XmlReader.Create( resx! );

        var xsd = XmlSchema.Read( reader, null )!;

        var ss = new XmlSchemaSet( new NameTable() );
        ss.Add( xsd );

        return ss;
    }


    /// <summary />
    public Company Company( string companyCode, XmlDocument xml )
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
    public Person Person( string companyCode, XmlDocument xml )
    {
        var ns = NamespaceManager();
        var p = new Person()
        {
            CompanyCode = companyCode,
            Id = xml.SelectSingleNode( " /c:person/c:id ", ns )?.InnerText,
            Username = xml.SelectSingleNode( " /c:person/c:username ", ns )?.InnerText,
            Name = xml.SelectSingleNode( " /c:person/c:name ", ns )!.InnerText,
            Expires = DateOnly.ParseExact( xml.SelectSingleNode( " /c:person/c:expires ", ns )!.InnerText, "yyyy-MM-dd" ),

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