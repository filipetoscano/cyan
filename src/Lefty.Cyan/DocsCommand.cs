using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

/// <summary />
[Command( "docs", Description = "Generates mark down documentation" )]
public class DocsCommand
{
    private readonly CyanConfiguration _config;
    private readonly ILogger<DocsCommand> _logger;

    /// <summary />
    public DocsCommand( IOptions<CyanConfiguration> config,
        ILogger<DocsCommand> logger )
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