using CliWrap;
using Lefty.Cyan.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Lefty.Cyan.Azure;

/// <summary />
public partial class AzService
{
    private readonly CyanConfiguration _config;
    private readonly string _organizationUrl;
    private readonly ILogger<AzService> _logger;


    /// <summary />
    public AzService( IOptions<CyanConfiguration> config,
        ILogger<AzService> logger )
    {
        _config = config.Value;
        _logger = logger;
        _organizationUrl = $"https://dev.azure.com/{_config.DevopsOrganization}";
    }


    /// <summary />
    private async Task<T> AzCli<T>( params string[] args )
    {
        var stdout = new StringBuilder();

        _logger.LogInformation( "az {Args}", string.Join( " ", args ) );

        await Cli
            .Wrap( "az" )
            .WithArguments( ( a ) =>
            {
                a.Add( args );
            } )
            .WithStandardOutputPipe( PipeTarget.ToStringBuilder( stdout ) )
            .ExecuteAsync();

        var json = stdout.ToString();
        _logger.LogDebug( "{Json}", json );

        return JsonSerializer.Deserialize<T>( json )!;
    }


    /// <summary />
    private Task<T> DevOps<T>( params string[] args )
    {
        var xargs = args.Concat( new[] { "--org", _organizationUrl } ).ToArray();
        return AzCli<T>( xargs );
    }
}