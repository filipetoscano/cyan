using Lefty.Cyan.Repository.Model;

namespace Lefty.Cyan.Repository;

public partial class RepositoryService
{
    /// <summary />
    public List<Company> DirectoryGet()
    {
        var idx = IndexGet();
        var list = new List<Company>();

        foreach ( var company in idx )
        {
            var c = ToCompany( company.Key, Company( company.Key ).Data );
            c.Persons = new List<Person>();

            foreach ( var person in company.Value )
            {
                var p = ToPerson( company.Key, Person( company.Key, person ).Data );
                c.Persons.Add( p );
            }

            if ( c.Persons.Count() == 0 )
                c.Persons = null;

            list.Add( c );
        }

        return list;
    }
}