using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Cyan.Model;

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


/// <summary />
public class CyanConfigurationValidation : IValidateOptions<CyanConfiguration>
{
    /// <summary />
    public ValidateOptionsResult Validate( string? name, CyanConfiguration options )
    {
        return ValidateOptionsResult.Fail( "Test" );
    }
}