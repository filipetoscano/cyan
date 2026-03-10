using System.Text.Json.Serialization;

namespace Lefty.Cyan.Repository.Model;

/// <summary />
public class Company
{
    /// <summary />
    public required string Code { get; set; }

    /// <summary />
    public required string Name { get; set; }

    /// <summary />
    public required string Description { get; set; }


    /// <summary />
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public List<Person>? Persons { get; set; }
}