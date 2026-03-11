using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class Group
{
    /// <summary />
    [JsonPropertyName( "descriptor" )]
    public required string Descriptor { get; set; }

    /// <summary />
    [JsonPropertyName( "description" )]
    public required string? Description { get; set; }

    /// <summary />
    [JsonPropertyName( "displayName" )]
    public required string DisplayName { get; set; }


    /// <summary />
    [JsonPropertyName( "_memberships" )]
    public List<GroupMember>? Memberships { get; set; }

    /// <summary />
    [JsonPropertyName( "_members" )]
    public List<string>? Members { get; set; }
}