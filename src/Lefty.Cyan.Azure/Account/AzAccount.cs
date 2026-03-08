using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Account;

/// <summary />
/// <remarks>
/// Response from `az account show`
/// </remarks>
public class AzAccount
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required Guid SubscriptionId { get; set; }

    /// <summary />
    [JsonPropertyName( "name" )]
    public required string SubscriptionName { get; set; }

    /// <summary />
    [JsonPropertyName( "tenantDisplayName" )]
    public string? TenantDisplayName { get; set; }

    /// <summary />
    [JsonPropertyName( "tenantDomainName" )]
    public string? TenantDomainName { get; set; }

    /// <summary />
    [JsonPropertyName( "tenantId" )]
    public required Guid TenantId { get; set; }

    /// <summary />
    [JsonPropertyName( "user" )]
    public AzAccountUser? User { get; set; }
}


/// <summary />
public class AzAccountUser
{
    /// <summary />
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }

    /// <summary />
    [JsonPropertyName( "type" )]
    public required string Type { get; set; }
}