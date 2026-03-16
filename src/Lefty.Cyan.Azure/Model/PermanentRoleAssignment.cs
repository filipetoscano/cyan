namespace Lefty.Cyan.Azure.Model;

/// <summary />
public class PermanentRoleAssignmentRoleAssignment
{
    /// <summary />
    public required string id { get; set; }

    /// <summary />
    public string? description { get; set; }


    /// <summary />
    public required string principalId { get; set; }

    /// <summary />
    public required string principalName { get; set; }

    /// <summary />
    public required string principalType { get; set; }


    /// <summary />
    public required string roleDefinitionId { get; set; }

    /// <summary />
    public required string roleDefinitionName { get; set; }
}