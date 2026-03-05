using System.Xml;

namespace Lefty.Cyan.Model;

public class Repository
{
    /// <summary />
    public required XmlDocument Azure { get; set; }

    /// <summary />
    public required XmlDocument DevOps { get; set; }

    /// <summary />
    public required XmlDocument DNS { get; set; }

    /// <summary />
    public required XmlDocument Jump { get; set; }

    /// <summary />
    public required List<Company> Companies { get; set; }
}