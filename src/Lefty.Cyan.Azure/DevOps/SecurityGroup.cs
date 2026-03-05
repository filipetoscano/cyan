using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class SecurityGroup
{
    /// <summary />
    [JsonPropertyName( "continuationToken" )]
    public required string? ContinuationToken { get; set; }

    /// <summary />
    [JsonPropertyName( "graphGroups" )]
    public required List<Group> Groups { get; set; } = default!;
}