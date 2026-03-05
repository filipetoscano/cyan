using Lefty.Cyan.Azure.DevOps;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public async Task<List<Project>> DevOpsProjectAsync(
        bool withRepos = false,
        bool withPipelines = false,
        bool withServiceConnectors = false )
    {
        /*
         * 
         */
        var q1 = await DevOps<ProjectListResponse>( "devops", "project", "list" );

        var projects = new List<Project>();
        projects.AddRange( q1.Projects );


        /*
         * 
         */
        foreach ( var p in projects )
        {
            if ( withRepos == true )
            {
                var q2 = await DevOps<List<Repository>>( "repos", "list", "--project", p.Name );

                if ( q2.Count > 0 )
                    p.Repositories = q2;
            }

            if ( withPipelines == true )
            {
                var q3 = await DevOps<List<Pipeline>>( "pipelines", "list", "--project", p.Name );

                p.Pipelines = new List<Pipeline>();

                foreach ( var pp in q3 )
                {
                    var rs4 = await AzCli<Pipeline>( "pipelines", "show",
                        "--id", pp.Id.ToString(),
                        "--project", p.Name );

                    p.Pipelines.Add( rs4 );
                }

                if ( p.Pipelines.Count() == 0 )
                    p.Pipelines = null;
            }

            if ( withServiceConnectors == true )
            {
                var q4 = await DevOps<List<ServiceConnector>>(
                    "devops", "service-endpoint", "list",
                    "--project", p.Name );

                if ( q4.Count > 0 )
                    p.ServiceConnectors = q4;
            }
        }

        return projects.ToList();
    }
}