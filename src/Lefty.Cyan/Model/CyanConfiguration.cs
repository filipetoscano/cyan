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
        var failures = new List<string>();

        if ( string.IsNullOrWhiteSpace( options.Root ) == true )
            failures.Add( "CYAN_ROOT env variable is required" );

        if ( Directory.Exists( options.Root ) == false )
            failures.Add( $"Directory {options.Root} does not exist" );

        if ( string.IsNullOrWhiteSpace( options.EntraDomain ) == true )
            failures.Add( "CYAN_ENTRA env variable is required" );

        if ( string.IsNullOrWhiteSpace( options.DevopsOrganization ) == true )
            failures.Add( "CYAN_DEVOPS_ORG env variable is required" );

        if ( failures.Count > 0 )
            return ValidateOptionsResult.Fail( failures );

        return ValidateOptionsResult.Success;
    }
}