using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
internal class TeamEx
{
    /// <summary />
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }
}