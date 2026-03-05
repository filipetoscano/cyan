using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class Project
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public Guid Id { get; set; }

    /// <summary />
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }

    /// <summary />
    [JsonPropertyName( "state" )]
    public required string State { get; set; }

    /// <summary />
    [JsonPropertyName( "description" )]
    public string? Description { get; set; }


    /// <summary />
    [JsonPropertyName( "_repositories" )]
    public List<Repository>? Repositories { get; set; }

    /// <summary />
    [JsonPropertyName( "_pipelines" )]
    public List<Pipeline>? Pipelines { get; set; }

    /// <summary />
    [JsonPropertyName( "_svcConnectors" )]
    public List<ServiceConnector>? ServiceConnectors { get; set; }

    /// <summary />
    [JsonPropertyName( "_groups" )]
    public List<Group>? Groups { get; set; }

    /// <summary />
    [JsonPropertyName( "_teams" )]
    public List<Team>? Teams { get; set; }
}