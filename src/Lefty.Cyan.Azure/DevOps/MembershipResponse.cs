namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class MembershipResponse
{
    /// <summary />
    public required List<Project> Projects { get; set; }

    /// <summary />
    public required List<User> Users { get; set; }
}