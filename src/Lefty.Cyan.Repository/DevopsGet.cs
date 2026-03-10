using Lefty.Cyan.Repository.Model;
using System.Xml;

namespace Lefty.Cyan.Repository;

public partial class RepositoryService
{
    /// <summary />
    public Devops DevopsGet()
    {
        var res = Validate( "devops", "system/devops.xml" );
        var xml = res.Data;

        var obj = new Devops()
        {
            Name = _config.DevopsOrganization,
            Projects = new List<Project>(),
        };

        foreach ( XmlElement projectElem in xml.SelectNodes( " /c:devops/c:project ", _mgr )! )
        {
            var p = new Project()
            {
                Name = projectElem.Attributes[ "name" ]!.Value,
                Description = projectElem.SelectSingleNode( " c:description ", _mgr )?.InnerText,
                Repositories = new List<ProjectRepository>(),
            };

            foreach ( XmlElement repoElem in projectElem.SelectNodes( " c:repository ", _mgr )! )
            {
                p.Repositories.Add( new ProjectRepository()
                {
                    Name = repoElem.Attributes[ "name" ]!.Value,
                } );
            }

            obj.Projects.Add( p );
        }

        return obj;
    }
}