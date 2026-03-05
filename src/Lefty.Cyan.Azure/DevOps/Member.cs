using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class Member
{
    /// <summary />
    [JsonPropertyName( "accessLevel" )]
    public required MemberAccessLevel AccessLevel { get; set; }

    /// <summary />
    [JsonPropertyName( "id" )]
    public Guid Id { get; set; }

    /// <summary />
    [JsonPropertyName( "user" )]
    public required User User { get; set; }
}