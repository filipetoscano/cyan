using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Account;

/// <summary />
/// <remarks>
/// Response from `az ad user show --id UPN`
/// </remarks>
public class EntraUser
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required Guid Id { get; set; }

    /// <summary />
    [JsonPropertyName( "displayName" )]
    public required string DisplayName { get; set; }

    /// <summary />
    [JsonPropertyName( "mail" )]
    public string? Email { get; set; }

    /// <summary />
    [JsonPropertyName( "userPrincipalName" )]
    public required string UserPrincipalName { get; set; }
}