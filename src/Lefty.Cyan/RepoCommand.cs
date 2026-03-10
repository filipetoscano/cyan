using McMaster.Extensions.CommandLineUtils;

namespace Lefty.Cyan;

/// <summary />
[Command( "repo", Description = "Repository browser" )]
[Subcommand( typeof( Repo.RepoPeopleCommand ) )]
public class RepoCommand
{
    /// <summary />
    public RepoCommand()
    {
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}