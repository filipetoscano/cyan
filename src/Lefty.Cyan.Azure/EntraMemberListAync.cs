using Lefty.Cyan.Azure.Account;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public Task<List<EntraMember>> EntraMemberListAync( string groupName )
    {
        return AzCli<List<EntraMember>>( "ad", "group", "member", "list", "--group", groupName );
    }
}