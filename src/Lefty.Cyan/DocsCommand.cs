using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Lefty.Cyan;

/// <summary />
[Command( "docs", Description = "Generates mark down documentation" )]
[Subcommand( typeof( Docs.ExcelCommand ) )]
public class DocsCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _repo;
    private readonly ILogger<DocsCommand> _logger;

    /// <summary />
    public DocsCommand( IOptions<CyanConfiguration> config,
        RepositoryService repo,
        ILogger<DocsCommand> logger )
    {
        _config = config.Value;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        var dir = _repo.DirectoryGet();

        var sb = new StringBuilder();
        sb.AppendLine( "People" );
        sb.AppendLine( "==========================================================================" );
        sb.AppendLine();
        sb.AppendLine( "| Company | Id | Username | Name | Expires |" );
        sb.AppendLine( "|---------|----|----------|------|---------|" );

        foreach ( var c in dir )
        {
            if ( c.Persons == null )
                continue;

            foreach ( var p in c.Persons )
            {
                sb.AppendLine( $"| {c.Name} | {M( p.Id )} | {M( p.Username )} | {p.Name} | {p.DateExpiry.ToString( "yyyy-MM-dd" )}" );
            }
        }

        sb.AppendLine();

        var dname = Path.Combine( _config.Root, "docs" );
        var fname = Path.Combine( dname, "People.md" );

        Directory.CreateDirectory( dname );
        File.WriteAllText( fname, sb.ToString() );

        return 0;
    }


    /// <summary />
    private static string M( string? value )
    {
        if ( value == null )
            return "";

        if ( value.Length == 0 )
            return "";

        return $"`{value}`";
    }
}