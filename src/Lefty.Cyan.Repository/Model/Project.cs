namespace Lefty.Cyan.Repository.Model;

/// <summary />
public class Project
{
    /// <summary />
    public Guid? Id { get; set; }

    /// <summary />
    public required string Name { get; set; }

    /// <summary />
    public string? Description { get; set; }

    /// <summary />
    public required List<string> Groups { get; set; }

    /// <summary />
    public required List<string> Teams { get; set; }

    /// <summary />
    public required List<ProjectRepository> Repositories { get; set; }
}