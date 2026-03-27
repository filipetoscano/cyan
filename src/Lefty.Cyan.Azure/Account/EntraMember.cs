using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Account;

/// <summary />
public class EntraMember
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required Guid Id { get; set; }

    /// <summary />
    [JsonPropertyName( "userPrincipalName" )]
    public required string UserPrincipalName { get; set; }
}