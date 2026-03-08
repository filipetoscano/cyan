using Lefty.Cyan.Azure;
using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Lefty.Cyan;

/// <summary />
[Command( "plan", Description = "Generates scripts" )]
public class PlanCommand
{
    private readonly CyanConfiguration _config;
    private readonly AzService _az;
    private readonly RepositoryService _repo;
    private readonly ILogger<PlanCommand> _logger;

    /// <summary />
    public PlanCommand( IOptions<CyanConfiguration> config,
        AzService az,
        RepositoryService repo,
        ILogger<PlanCommand> logger )
    {
        _config = config.Value;
        _az = az;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        /*
         * 
         */
        if ( File.Exists( Path.Combine( _config.Root, "cyan.xsd" ) ) == false )
        {
            _logger.LogError( "Invalid root, does not contain cyan.xsd file" );
            return 1;
        }


        /*
         * 
         */
        await PlanDevops();
        await PlanDevopsRbac();
        await PlanAzureRbac();

        return 0;
    }


    /// <summary />
    private async Task PlanAzureRbac()
    {
        // TODO

        await Task.Yield();
    }


    /// <summary />
    private async Task PlanDevops()
    {
        /*
         * 
         */
        var actual = await _az.DevOpsProjectAsync( true, false, false );
        var expected = _repo.DevopsGet();


        /*
         * Detect differences
         */
        var sb = new StringBuilder();

        foreach ( var proj in expected.Projects )
        {
            var m = actual.SingleOrDefault( x => x.Name == proj.Name );

            if ( m == null )
            {
                _logger.LogInformation( "Add project {Project}", proj.Name );
                sb.AppendFormat( @"az devops project create --org ""https://dev.azure.com/{0}"" --name {1} --source-control git --visibility private ", _config.DevopsOrganization, proj.Name );
                sb.AppendLine();
            }

            if ( proj.Description != null && proj.Description != m?.Description )
            {
                _logger.LogInformation( "Update project {Project} description", proj.Name );
                sb.AppendFormat( @"az devops project update --org ""https://dev.azure.com/{0}"" --project {1} --description ""{2}"" ", _config.DevopsOrganization, proj.Name, proj.Description );
                sb.AppendLine();
            }

            foreach ( var repo in proj.Repositories )
            {
                if ( m?.Repositories?.Count( x => x.Name == repo.Name ) == 1 )
                    continue;

                _logger.LogInformation( "Add repository {Project}/{Repository}", proj.Name, repo.Name );
                sb.AppendFormat( @"az repos create --org ""https://dev.azure.com/{0}"" --project {1} --name {2}", _config.DevopsOrganization, proj.Name, repo.Name );
                sb.AppendLine();
            }
        }


        /*
         * 
         */
        foreach ( var proj in actual )
        {
            var m = expected.Projects.SingleOrDefault( x => x.Name == proj.Name );

            if ( m == null )
                _logger.LogWarning( "Project {Project} exists in org, but not defined", proj.Name );

            if ( proj.Repositories == null )
                continue;

            foreach ( var repo in proj.Repositories )
            {
                var m2 = m?.Repositories.SingleOrDefault( x => x.Name == repo.Name );

                if ( m2 == null )
                    _logger.LogWarning( "Repository {Project}/{Repository} exists in org, but not defined", proj.Name, repo.Name );
            }
        }


        /*
         * 
         */
        if ( sb.Length > 0 )
        {
            _logger.LogInformation( "Write plan-devops.sh" );
            File.WriteAllText( "plan-devops.sh", sb.ToString() );
        }
    }


    /// <summary />
    private async Task PlanDevopsRbac()
    {
        // TODO

        await Task.Yield();
    }
}