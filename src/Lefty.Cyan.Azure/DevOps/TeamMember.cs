using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class TeamMember
{
    /// <summary />
    [JsonPropertyName( "teamDescriptor" )]
    public required string TeamDescriptor { get; set; }

    /// <summary />
    [JsonPropertyName( "userDescriptor" )]
    public required string UserDescriptor { get; set; }
}