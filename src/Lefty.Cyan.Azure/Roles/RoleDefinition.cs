using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.Roles;

/// <summary />
public class RoleDefinition
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required string Id { get; set; }

    /// <summary />
    [JsonPropertyName( "roleName" )]
    public required string Name { get; set; }
}