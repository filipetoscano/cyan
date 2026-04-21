using Microsoft.Extensions.Logging;

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
            "logAnalyticsWorkspace" => "Microsoft.OperationalInsights/workspaces",
            "netAppFiles" => "Microsoft.NetApp/netAppAccounts",
            "postgres" => "Microsoft.DBforPostgreSQL/flexibleServers",
            "redis" => "Microsoft.Cache/redis",
            "sqlServer" => "Microsoft.Sql/servers",
            "storage" => "Microsoft.Storage/storageAccounts",

            _ => throw new NotSupportedException( resourceType )
        };
    }


    private static Dictionary<string, string> _api = new Dictionary<string, string>();


    /// <summary />
    private async Task<string> ApiVersionFor( string resourceType )
    {
        if ( _api.TryGetValue( resourceType, out var ver ) == true )
            return ver;

        ver = await GetApiVersionFor( resourceType );

        _logger.LogInformation( "{ResourceType} API = {Version}", resourceType, ver );
        _api.TryAdd( resourceType, ver );

        return ver;
    }


    /// <summary />
    private async Task<string> GetApiVersionFor( string resourceType )
    {
        var p = resourceType.Split( '/' );
        var ns = p[ 0 ];
        var rt = p[ 1 ];

        var versions = await AzCli<List<string>>( "provider", "show",
            "--namespace", ns,
            "--query", $"resourceTypes[?resourceType=='{rt}'].apiVersions[]" );

        return versions.First( v => v.Contains( "preview" ) == false );
    }


    /// <summary />
    public async Task<string> ResourceGetAsync( string resourceType, string resourceName, string resourceGroup, string subscription )
    {
        var rt = ResourceTypeFor( resourceType );
        var version = await ApiVersionFor( rt );

        var r = await AzCli<ResourceId>( "resource", "show",
            "--resource-type", rt,
            "--name", resourceName,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--api-version", version );

        return r.Id;
    }
}