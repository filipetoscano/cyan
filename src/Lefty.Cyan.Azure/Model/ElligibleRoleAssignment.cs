namespace Lefty.Cyan.Azure.Model;

/// <summary />
public class ElligibleRoleAssignmentListResponse
{
    /// <summary />
    public required List<RoleEligibilityScheduleInstances> value { get; set; }
}


/// <summary />
public class RoleEligibilityScheduleInstances
{
    /// <summary />
    public required string id { get; set; }

    /// <summary />
    public required RoleEligibilityScheduleInstancesProps properties { get; set; }
}


/// <summary />
public class RoleEligibilityScheduleInstancesProps
{
    /// <summary />
    public string? memberType { get; set; }

    /// <summary />
    public string? principalType { get; set; }

    /// <summary />
    public RoleEligibilityScheduleInstancesExpanded? expandedProperties { get; set; }
}


/// <summary />
public class RoleEligibilityScheduleInstancesExpanded
{
    /// <summary />
    public UserPrincipal? principal { get; set; }

    /// <summary />
    public RoleDefinition? roleDefinition { get; set; }
}


/// <summary />
public class UserPrincipal
{
    /// <summary />
    public string? id { get; set; }

    /// <summary />
    public string? type { get; set; }

    /// <summary />
    public string? userPrincipalName { get; set; }
}


/// <summary />
public class RoleDefinition
{
    /// <summary />
    public string? id { get; set; }

    /// <summary />
    public string? displayName { get; set; }

    /// <summary />
    public string? type { get; set; }
}