using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Account;

/// <summary />
public class EntraGroup
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required Guid Id { get; set; }

    /// <summary />
    [JsonPropertyName( "displayName" )]
    public required string DisplayName { get; set; }

    /// <summary />
    [JsonPropertyName( "description" )]
    public string? Description { get; set; }

    /// <summary />
    [JsonPropertyName( "securityIdentifier" )]
    public string? SecurityIdentifier { get; set; }
}