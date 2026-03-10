using McMaster.Extensions.CommandLineUtils;

namespace Lefty.Cyan;

/// <summary />
[Command( "add", Description = "Adds a company or person" )]
[Subcommand( typeof( Add.AddCompanyCommand ) )]
[Subcommand( typeof( Add.AddFromCommand ) )]
[Subcommand( typeof( Add.AddPersonCommand ) )]
public class AddCommand
{
    /// <summary />
    public AddCommand()
    {
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}