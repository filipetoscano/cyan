using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Account;

/// <summary />
/// <remarks>
/// Response from `az account show`
/// </remarks>
public class AzAccount
{
    /// <summary />
    [JsonPropertyName( "tenantDisplayName" )]
    public required string TenantDisplayName { get; set; }

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