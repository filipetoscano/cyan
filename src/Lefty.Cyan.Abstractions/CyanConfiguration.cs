using System.ComponentModel.DataAnnotations;

namespace Lefty.Cyan;

/// <summary />
public class CyanConfiguration
{
    /// <summary>
    /// Configuration root.
    /// </summary>
    [Required]
    public required string Root { get; set; }

    /// <summary>
    /// Entra email identifier.
    /// </summary>
    [Required]
    public required string EntraDomain { get; set; }

    /// <summary>
    /// Name of Azure Devops organization.
    /// </summary>
    [Required]
    public required string DevopsOrganization { get; set; }
}