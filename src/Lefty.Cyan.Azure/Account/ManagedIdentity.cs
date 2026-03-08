using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Account;

/// <summary />
public class ManagedIdentity
{
    /// <summary />
    [JsonPropertyName( "clientId" )]
    public required Guid ClientId { get; set; }

    /// <summary />
    [JsonPropertyName( "id" )]
    public required string Id { get; set; }

    /// <summary />
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }

    /// <summary />
    [JsonPropertyName( "principalId" )]
    public required Guid PrincipalId { get; set; }

    /// <summary />
    [JsonPropertyName( "resourceGroup" )]
    public required string ResourceGroup { get; set; }
}