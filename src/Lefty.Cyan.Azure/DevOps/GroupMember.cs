using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class GroupMember
{
    /// <summary />
    [JsonPropertyName( "groupDescriptor" )]
    public required string GroupDescriptor { get; set; }

    /// <summary />
    [JsonPropertyName( "userDescriptor" )]
    public required string UserDescriptor { get; set; }
}