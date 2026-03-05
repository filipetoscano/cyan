using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class Repository
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required Guid Id { get; set; }

    /// <summary />
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }

    /// <summary />
    [JsonPropertyName( "defaultBranch" )]
    public string? DefaultBranch { get; set; }
}