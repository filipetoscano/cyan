namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class Organization
{
    /// <summary />
    public List<Project> Projects { get; set; } = default!;

    /// <summary />
    public List<Member> Members { get; set; } = default!;
}