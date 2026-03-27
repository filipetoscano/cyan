using System.Xml;

namespace Lefty.Cyan.Repository;

public partial class RepositoryService
{
    /// <summary />
    public Result<XmlDocument> Azure()
    {
        return Validate( "azure", "system/azure.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Devops()
    {
        return Validate( "devops", "system/devops.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Dns()
    {
        return Validate( "dns", "system/dns.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Entra()
    {
        return Validate( "entra", "system/entra.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Firewall()
    {
        return Validate( "firewall", "system/firewall.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Jump()
    {
        return Validate( "jump", "system/jump.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Company( string companyCode )
    {
        return Validate( "company", $"people/{companyCode}/index.xml" );
    }


    /// <summary />
    public Result<XmlDocument> Person( string companyCode, string personName )
    {
        return Validate( "person", $"people/{companyCode}/{personName}/index.xml" );
    }


    /// <summary />
    public Result<XmlDocument> PersonRbac( string companyCode, string personName, string rbac = "rbac" )
    {
        return Validate( "rbac", $"people/{companyCode}/{personName}/{rbac}.xml" );
    }
}