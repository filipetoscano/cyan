using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
[JsonConverter( typeof( JsonStringEnumConverter<ProjectState> ) )]
public enum ProjectState
{
    /// <summary>
    /// Project is in the process of being created.
    /// </summary>
    [JsonStringEnumMemberName( "new" )]
    New,

    /// <summary>
    /// Project is completely created and ready to use.s
    /// </summary>
    [JsonStringEnumMemberName( "wellFormed" )]
    WellFormed,

    /// <summary>
    /// Project is in the process of being deleted.
    /// </summary>
    [JsonStringEnumMemberName( "deleting" )]
    Deleting,

    /// <summary>
    /// Project has been queued for creation, but the process has not yet started.
    /// </summary>
    [JsonStringEnumMemberName( "createPending" )]
    CreatePending,

    /// <summary>
    /// Project has been deleted.
    /// </summary>
    [JsonStringEnumMemberName( "deleted" )]
    Deleted,
}