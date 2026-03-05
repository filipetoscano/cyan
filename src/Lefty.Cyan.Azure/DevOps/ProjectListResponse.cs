using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class ProjectListResponse
{
    /// <summary />
    [JsonPropertyName( "continuationToken" )]
    public required string? ContinuationToken { get; set; }

    /// <summary />
    [JsonPropertyName( "value" )]
    public required List<Project> Projects { get; set; } = default!;
}