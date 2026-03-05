using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class GroupMembership
{
    /// <summary />
    [JsonPropertyName( "descriptor" )]
    public required string Descriptor { get; set; }

    /// <summary />
    [JsonPropertyName( "subjectKind" )]
    public required string SubjectKind { get; set; }
}