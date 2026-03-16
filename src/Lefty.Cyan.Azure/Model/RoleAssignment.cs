namespace Lefty.Cyan.Azure;

/// <summary />
public class RoleAssignment
{
    /// <summary />
    public required string Id { get; set; }

    /// <summary />
    public string? Description { get; set; }

    /// <summary />
    public required string PrincipalName { get; set; }

    /// <summary />
    public required string RoleId { get; set; }

    /// <summary />
    public required string Role { get; set; }
}