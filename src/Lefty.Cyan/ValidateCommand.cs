using Lefty.Cyan.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

/// <summary />
[Command( "validate", Description = "Validate repository objects" )]
public class ValidateCommand
{
    /// <summary />
    public ValidateCommand( IOptions<CyanConfiguration> config )
    {
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}