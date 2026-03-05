using Lefty.Cyan.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan.Services;

/// <summary />
public class AzService
{
    private readonly CyanConfiguration _config;
    private readonly ILogger<AzService> _logger;


    /// <summary />
    public AzService( IOptions<CyanConfiguration> config,
        ILogger<AzService> logger )
    {
        _config = config.Value;
        _logger = logger;
    }
}