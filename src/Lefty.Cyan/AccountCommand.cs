using Lefty.Cyan.Azure;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Lefty.Cyan;

/// <summary />
[Command( "account", Description = "Show account to Azure tenant" )]
public class AccountCommand
{
    private readonly AzService _svc;
    private readonly ILogger<AccountCommand> _logger;


    /// <summary />
    public AccountCommand( AzService svc,
        ILogger<AccountCommand> logger )
    {
        _svc = svc;
        _logger = logger;
    }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        var acc = await _svc.AccountGetAsync();

        _logger.LogInformation( "Tenant = {Tenant}", acc.TenantDisplayName );
        _logger.LogInformation( "User = {UserName}", acc.User?.Name );

        return 0;
    }
}