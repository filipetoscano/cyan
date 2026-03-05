using Lefty.Cyan.Model;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

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