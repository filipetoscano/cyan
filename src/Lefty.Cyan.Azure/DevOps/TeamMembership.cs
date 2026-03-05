using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class TeamMembership
{
    /// <summary />
    [JsonPropertyName( "identity" )]
    public required TeamMembershipIdentity Identity { get; set; }
}


/// <summary />
public class TeamMembershipIdentity
{
    /// <summary />
    [JsonPropertyName( "descriptor" )]
    public required string Descriptor { get; set; }

    /// <summary />
    [JsonPropertyName( "id" )]
    public required Guid Id { get; set; }
}