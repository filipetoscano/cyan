using Lefty.Cyan.Azure.Account;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public Task<AzAccount> AccountGetAsync()
    {
        return AzCli<AzAccount>( "account", "show" );
    }
}