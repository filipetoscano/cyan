using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class MemberAccessLevel
{
    /// <summary />
    [JsonPropertyName( "accountLicenseType" )]
    public required DevOpsAccessType AccountLicenseType { get; set; }
}

/// <summary />
[JsonConverter( typeof( JsonStringEnumConverter<DevOpsAccessType> ) )]
public enum DevOpsAccessType
{
    [JsonStringEnumMemberName( "express" )]
    Basic,

    [JsonStringEnumMemberName( "advanced" )]
    BasicAndTest,

    [JsonStringEnumMemberName( "stakeholder" )]
    Stakeholder,
}