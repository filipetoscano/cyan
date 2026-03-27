using Lefty.Cyan.Azure.Account;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public async Task<EntraGroup> EntraGroupGetAsync( string groupName )
    {
        var groups = await AzCli<List<EntraGroup>>( "ad", "group", "list", "--display-name", groupName );

        return groups.Single();
    }
}