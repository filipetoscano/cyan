namespace Lefty.Cyan.Repository.Model;

/// <summary />
public class Person
{
    /// <summary />
    public required string CompanyCode { get; set; }

    /// <summary />
    public string? Id { get; set; }

    /// <summary />
    public string? Username { get; set; }

    /// <summary />
    public string? PrincipalName { get; set; }

    /// <summary />
    public required string Name { get; set; }

    /// <summary />
    public required DateOnly Expires { get; set; }

    /// <summary />
    public required bool IsEnabled { get; set; }

    /// <summary />
    public string? Email { get; set; }

    /// <summary />
    public string? Phone { get; set; }

    /// <summary />
    public string? Role { get; set; }
}