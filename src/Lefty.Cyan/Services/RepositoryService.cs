using Lefty.Cyan.Model;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Cyan.Services;

/// <summary />
public class RepositoryService
{
    /// <summary />
    public RepositoryService()
    {
    }


    /// <summary />
    public XmlNamespaceManager NamespaceManager()
    {
        var mgr = new XmlNamespaceManager( new NameTable() );
        mgr.AddNamespace( "c", "urn:cyan" );

        return mgr;
    }


    /// <summary />
    public string SkeletonGet( string skel )
    {
        var rest = skel.Replace( "/", "." );

        using var resx = typeof( Program ).Assembly.GetManifestResourceStream( $"Lefty.Cyan.Resources.{rest}" );
        using var sr = new StreamReader( resx! );

        return sr.ReadToEnd();
    }


    /// <summary />
    public string SchemaGet()
    {
        using var resx = typeof( Program ).Assembly.GetManifestResourceStream( "Lefty.Cyan.Resources.cyan.xsd" );
        using var sr = new StreamReader( resx! );

        return sr.ReadToEnd();
    }


    /// <summary />
    public XmlSchemaSet SchemaSetGet()
    {
        using var resx = typeof( Program ).Assembly.GetManifestResourceStream( "Lefty.Cyan.Resources.cyan.xsd" );
        using var reader = XmlReader.Create( resx! );

        var xsd = XmlSchema.Read( reader, null )!;

        var ss = new XmlSchemaSet( new NameTable() );
        ss.Add( xsd );

        return ss;
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

        return p;
    }
}