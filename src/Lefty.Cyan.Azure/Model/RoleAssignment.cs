using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure;

/// <summary />
public class RoleAssignment
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required string Id { get; set; }

    /// <summary />
    [JsonPropertyName( "description" )]
    public string? Description { get; set; }


    /// <summary />
    [JsonPropertyName( "principalId" )]
    public required string PrincipalId { get; set; }

    /// <summary />
    [JsonPropertyName( "principalName" )]
    public required string PrincipalName { get; set; }

    /// <summary />
    [JsonPropertyName( "principalType" )]
    public required string PrincipalType { get; set; }


    /// <summary />
    [JsonPropertyName( "roleDefinitionId" )]
    public required string RoleId { get; set; }

    /// <summary />
    [JsonPropertyName( "roleDefinitionName" )]
    public required string Role { get; set; }
}