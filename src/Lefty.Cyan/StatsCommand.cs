using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace Lefty.Cyan;

/// <summary />
[Command( "stats", Description = "" )]
public class StatsCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _repo;
    private readonly ILogger<PlanCommand> _logger;

    /// <summary />
    public StatsCommand( IOptions<CyanConfiguration> config,
        RepositoryService repo,
        ILogger<PlanCommand> logger )
    {
        _config = config.Value;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute()
    {
        /*
         * 
         */
        var dir = _repo.DirectoryGet();
        var mgr = _repo.NamespaceManager();

        var azRbac = 0;
        var doUsers = 0;
        var doRbac = 0;
        var jumpRbac = 0;

        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            if ( p.IsEnabled == false )
                continue;

            var rbac = _repo.PersonRbac( p.CompanyCode, p.Name ).Data;

            azRbac += rbac.SelectNodes( " /c:rbac/c:azure/c:* ", mgr )?.Count ?? 0;
            doUsers += rbac.SelectNodes( " /c:rbac/c:devops:* ", mgr )?.Count ?? 0;
            doRbac += rbac.SelectNodes( " /c:rbac/c:devops/c:project/c:group | /c:rbac/c:devops/c:project/c:team ", mgr )?.Count ?? 0;
            jumpRbac += rbac.SelectNodes( " /c:rbac/c:jump ", mgr )?.Count ?? 0;
        }


        /*
         * 
         */
        var stats = new Table();
        stats.Border = TableBorder.SimpleHeavy;
        stats.AddColumn( "Key" );
        stats.AddColumn( "Value", col => col.RightAligned() );

        stats.AddRow( "Companies", dir.Count.ToString() );
        stats.AddRow( "Persons", dir.SelectMany( x => x.Persons ?? [] ).Count().ToString() );
        stats.AddRow( "Azure RBAC", azRbac.ToString() );
        stats.AddRow( "DevOps Users", doUsers.ToString() );
        stats.AddRow( "DevOps RBAC", doRbac.ToString() );
        stats.AddRow( "Jump access", jumpRbac.ToString() );

        AnsiConsole.Write( stats );

        return 0;
    }
}