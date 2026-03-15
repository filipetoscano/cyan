namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public async Task<string> SubscriptionGetAsync( string subscription )
    {
        var r = await AzCli<ResourceId>( "account", "show",
            "--name", subscription );

        return "/subscriptions/" + r.Id;
    }


    /// <summary />
    public async Task<string> ResourceGroupGetAsync( string resourceGroup, string subscription )
    {
        var r = await AzCli<ResourceId>( "group", "show",
            "--name", resourceGroup,
            "--subscription", subscription );

        return r.Id;
    }


    /// <summary />
    public string ResourceTypeFor( string resourceType )
    {
        return resourceType switch
        {
            "acr" => "Microsoft.ContainerRegistry/registries",
            "aks" => "Microsoft.ContainerService/managedClusters",
            "appInsights" => "Microsoft.Insights/components",
            "dataBricks" => "Microsoft.Databricks/workspaces",
            "eventHub" => "Microsoft.EventHub/namespaces",
            "keyVault" => "Microsoft.KeyVault/vaults",
            "netAppFiles" => "Microsoft.NetApp/netAppAccounts",
            "postgres" => "Microsoft.DBforPostgreSQL/flexibleServers",
            "redis" => "Microsoft.Cache/redis",
            "sqlServer" => "Microsoft.Sql/servers",
            "storage" => "Microsoft.Storage/storageAccounts",

            _ => throw new NotSupportedException( resourceType )
        };
    }


    /// <summary />
    public async Task<string> ResourceGetAsync( string resourceType, string resourceName, string resourceGroup, string subscription )
    {
        var rt = ResourceTypeFor( resourceType );

        var r = await AzCli<ResourceId>( "resource", "show",
            "--resource-type", rt,
            "--name", resourceName,
            "--resource-group", resourceGroup,
            "--subscription", subscription );

        return r.Id;
    }
}