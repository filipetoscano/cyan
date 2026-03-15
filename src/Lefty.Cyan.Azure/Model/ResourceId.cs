using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure;

/// <summary />
public class ResourceId
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required string Id { get; set; }
}