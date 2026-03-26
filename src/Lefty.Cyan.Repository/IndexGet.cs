namespace Lefty.Cyan.Repository;

public partial class RepositoryService
{
    /// <summary />
    public Dictionary<string, List<string>> IndexGet()
    {
        var idx = new Dictionary<string, List<string>>();
        var dir = new DirectoryInfo( Path.Combine( _config.Root, "people" ) );

        foreach ( var corpDir in dir.GetDirectories().OrderBy( x => x.Name ) )
        {
            var l = new List<string>();

            foreach ( var personDir in corpDir.GetDirectories().OrderBy( x => x.Name ) )
                l.Add( personDir.Name );

            idx.Add( corpDir.Name, l );
        }

        return idx;
    }
}