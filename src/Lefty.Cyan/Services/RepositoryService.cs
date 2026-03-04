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
    public XmlNamespaceManager Manager()
    {
        var mgr = new XmlNamespaceManager( new NameTable() );
        mgr.AddNamespace( "c", "urn:cyan" );

        return mgr;
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
}