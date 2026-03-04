using Lefty.Cyan.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

/// <summary />
[Command( "init", Description = "Initializes repository files (if missing)" )]
public class InitCommand
{
    private readonly CyanConfiguration _config;
    private readonly ILogger<InitCommand> _logger;

    /// <summary />
    public InitCommand( IOptions<CyanConfiguration> config,
        ILogger<InitCommand> logger )
    {
        _config = config.Value;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        // TODO

        return 0;
    }
}