namespace Lefty.Cyan.Repository.Model;

/// <summary />
public class Devops
{
    /// <summary />
    public required string Name { get; set; }

    /// <summary />
    public required List<Project> Projects { get; set; }
}