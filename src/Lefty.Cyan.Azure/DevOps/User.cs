using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class User
{
    /// <summary />
    [JsonPropertyName( "descriptor" )]
    public required string Descriptor { get; set; }

    /// <summary />
    [JsonPropertyName( "principalName" )]
    public required string PrincipalName { get; set; }
}