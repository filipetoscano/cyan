using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class MemberListResponse
{
    /// <summary />
    [JsonPropertyName( "continuationToken" )]
    public required string ContinuationToken { get; set; }

    /// <summary />
    [JsonPropertyName( "members" )]
    public required List<Member> Members { get; set; }

    /// <summary />
    [JsonPropertyName( "totalCount" )]
    public required int TotalCount { get; set; }
}