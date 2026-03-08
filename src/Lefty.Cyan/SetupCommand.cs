using Lefty.Cyan.Azure;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Cyan;

/// <summary />
public class SetupCommand
{
    private readonly CyanConfiguration _config;
    private readonly AzService _az;
    private readonly ILogger<SetupCommand> _logger;

    /// <summary />
    public SetupCommand( IOptions<CyanConfiguration> config,
        AzService az,
        ILogger<SetupCommand> logger )
    {
        _config = config.Value;
        _az = az;
        _logger = logger;
    }


    /// <summary />
    [Required]
    [Argument( 0, Description = "Managed identity name" )]
    public string? Name { get; set; }

    /// <summary />
    [Required]
    [Argument( 1, Description = "Resource group name" )]
    public string? ResourceGroup { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        var mi = await _az.ManagedIdentityGetAsync( this.Name!, this.ResourceGroup! );

        await _az.ManagedIdentityAdd( mi.Name );

        return 0;
    }
}