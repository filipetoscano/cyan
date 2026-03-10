using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Json;
using System.Text.Json;

namespace Lefty.Cyan.Repo;

/// <summary />
[Command( "people", Description = "Adds a company" )]
public class RepoPeopleCommand
{
    private readonly RepositoryService _repo;
    private readonly ILogger<RepoPeopleCommand> _logger;

    /// <summary />
    public RepoPeopleCommand( RepositoryService repo,
        ILogger<RepoPeopleCommand> logger )
    {
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    [Argument( 0, Description = "Only from company (code)" )]
    public string? CompanyCode { get; set; }

    /// <summary />
    [Option( "--json", CommandOptionType.NoValue, Description = "Emit results as Json" )]
    public bool AsJson { get; set; }

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Write JSON to output file" )]
    public string? OutputFile { get; set; }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        /*
         * 
         */
        var dir = _repo.DirectoryGet();


        /*
         * 
         */
        if ( this.OutputFile != null )
        {
            var jso = new JsonSerializerOptions() { WriteIndented = true };
            var json = JsonSerializer.Serialize( dir, jso );

            File.WriteAllText( this.OutputFile, json );

            return 0;
        }


        /*
         * 
         */
        if ( this.AsJson == true )
        {
            var json = JsonSerializer.Serialize( dir );

            AnsiConsole.Write( new JsonText( json ) );

            return 0;
        }


        /*
         * 
         */
        var table = new Table();
        table.Border = TableBorder.SimpleHeavy;
        table.AddColumn( "Company" );
        table.AddColumn( "Id" );
        table.AddColumn( "Username" );
        table.AddColumn( "Name" );
        table.AddColumn( "Expires" );

        var red = new Style( foreground: Color.Red );
        var yellow = new Style( foreground: Color.Yellow );
        var never = new DateOnly( 9999, 12, 31 );

        foreach ( var company in dir )
        {
            if ( company.Persons == null )
                continue;

            foreach ( var person in company.Persons )
            {
                var expiresValue = person.DateExpiry.ToString( "yyyy-MM-dd" );
                var expiresStyle = default( Style? );

                //if ( company.DateExpiry == never )
                //    expiresStyle = yellow;
                //else if ( _rules.ExpiringSoon( company.DateExpiry ) == true )
                //    expiresStyle = red;

                table.AddRow(
                   new Markup( company.Code ),
                   new Markup( person.Id?.ToString() ?? "" ),
                   new Markup( person.Username?.ToString() ?? "" ),
                   new Markup( person.Name ),
                   new Markup( expiresValue, expiresStyle )
                );
            }
        }

        AnsiConsole.Write( table );

        return 0;
    }
}