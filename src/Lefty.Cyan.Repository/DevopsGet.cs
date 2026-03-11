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

        foreach ( var projectElem in xml.SelectNodes( " /c:devops/c:project ", _mgr )!.OfType<XmlElement>() )
        {
            var p = new Project()
            {
                Name = projectElem.Attributes[ "name" ]!.Value,
                Description = projectElem.SelectSingleNode( " c:description ", _mgr )?.InnerText,
                Groups = new List<string>(),
                Teams = new List<string>(),
                Repositories = new List<ProjectRepository>(),
            };

            foreach ( var elem in projectElem.SelectNodes( " c:group ", _mgr )!.OfType<XmlElement>() )
            {
                p.Groups.Add( elem.Attributes[ "name" ]!.Value );
            }

            foreach ( var elem in projectElem.SelectNodes( " c:team ", _mgr )!.OfType<XmlElement>() )
            {
                p.Teams.Add( elem.Attributes[ "name" ]!.Value );
            }

            foreach ( var elem in projectElem.SelectNodes( " c:repository ", _mgr )!.OfType<XmlElement>() )
            {
                p.Repositories.Add( new ProjectRepository()
                {
                    Name = elem.Attributes[ "name" ]!.Value,
                } );
            }

            obj.Projects.Add( p );
        }

        return obj;
    }
}