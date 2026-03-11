using Lefty.Cyan.Azure.DevOps;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public class ProjectOptions
    {
        /// <summary />
        public bool WithRepositories { get; set; }

        /// <summary />
        public bool WithGroups { get; set; }

        /// <summary />
        public bool WithPipelines { get; set; }

        /// <summary />
        public bool WithServiceConnectors { get; set; }


        /// <summary />
        public static ProjectOptions All()
        {
            return new ProjectOptions()
            {
                WithRepositories = true,
                WithGroups = true,
                WithPipelines = true,
                WithServiceConnectors = true,
            };
        }
    }



    /// <summary />
    public async Task<List<Project>> DevOpsProjectAsync( ProjectOptions? options = null )
    {
        /*
         * 
         */
        var opt = options ?? new ProjectOptions();

        var q1 = await DevOps<ProjectListResponse>( "devops", "project", "list" );

        var projects = new List<Project>();
        projects.AddRange( q1.Projects );


        /*
         * 
         */
        foreach ( var p in projects )
        {
            if ( opt.WithRepositories == true )
            {
                var q2 = await DevOps<List<Repository>>( "repos", "list", "--project", p.Name );

                if ( q2.Count > 0 )
                    p.Repositories = q2;
            }

            if ( opt.WithGroups == true )
            {
                var q2 = await DevOps<List<TeamEx>>( "devops", "team", "list", "--project", p.Name );

                var q3 = await DevOps<SecurityGroup>(
                    "devops", "security", "group", "list",
                    "--scope", "project", "--project", p.Name );

                p.Teams = new List<Team>();
                p.Groups = new List<Group>();

                foreach ( var j in q3.Groups )
                {
                    if ( q2.SingleOrDefault( x => x.Name == j.DisplayName ) != null )
                    {
                        p.Teams.Add( new Team()
                        {
                            Descriptor = j.Descriptor,
                            DisplayName = j.DisplayName,
                            Description = j.Description,
                        } );
                    }
                    else
                    {
                        p.Groups.Add( new Group()
                        {
                            Descriptor = j.Descriptor,
                            DisplayName = j.DisplayName,
                            Description = j.Description,
                        } );
                    }
                }
            }

            if ( opt.WithPipelines == true )
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

            if ( opt.WithServiceConnectors == true )
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