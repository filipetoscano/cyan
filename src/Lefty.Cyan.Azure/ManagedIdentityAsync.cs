using Lefty.Cyan.Azure.Account;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public Task<ManagedIdentity> ManagedIdentityGetAsync( string name, string resourceGroup )
    {
        return AzCli<ManagedIdentity>( "identity", "show", "--name", name, "--resource-group", resourceGroup );
    }


    /// <summary />
    public async Task ManagedIdentityAdd( string name )
    {
        await DevOps<ManagedIdentity>( "devops", "user", "add",
            "--email-id", name,
            "--license-type", "express" );
    }
}