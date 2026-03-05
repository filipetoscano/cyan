using Lefty.Cyan.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

/// <summary />
[Command( "plan", Description = "Generates scripts" )]
public class PlanCommand
{
    private readonly CyanConfiguration _config;
    private readonly ILogger<PlanCommand> _logger;

    /// <summary />
    public PlanCommand( IOptions<CyanConfiguration> config,
        ILogger<PlanCommand> logger )
    {
        _config = config.Value;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        /*
         * 
         */
        if ( File.Exists( Path.Combine( _config.Root, "cyan.xsd" ) ) == false )
        {
            _logger.LogError( "Invalid root, does not contain cyan.xsd file" );
            return 1;
        }


        return 0;
    }
}