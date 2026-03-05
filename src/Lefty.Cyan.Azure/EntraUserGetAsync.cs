using Lefty.Cyan.Azure.Account;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public Task<EntraUser> EntraUserGetAsync( string upn )
    {
        return AzCli<EntraUser>( "ad", "user", "show", "--id", upn );
    }
}