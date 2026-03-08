using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

/// <summary />
[Command( "add", Description = "Adds a company or person" )]
[Subcommand( typeof( AddCompanyCommand ) )]
[Subcommand( typeof( AddPersonCommand ) )]
public class AddCommand
{
    private readonly CyanConfiguration _config;
    private readonly ILogger<AddCommand> _logger;

    /// <summary />
    public AddCommand( IOptions<CyanConfiguration> config,
        ILogger<AddCommand> logger )
    {
        _config = config.Value;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}