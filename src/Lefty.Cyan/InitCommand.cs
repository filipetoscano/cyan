using Lefty.Cyan.Model;
using Lefty.Cyan.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lefty.Cyan;

/// <summary />
[Command( "init", Description = "Initializes repository files (if missing)" )]
public class InitCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _svc;
    private readonly ILogger<InitCommand> _logger;

    /// <summary />
    public InitCommand( IOptions<CyanConfiguration> config,
        RepositoryService svc,
        ILogger<InitCommand> logger )
    {
        _config = config.Value;
        _svc = svc;
        _logger = logger;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        var b = true;

        b = b && FileEnsure( "system/azure.xml" );
        b = b && FileEnsure( "system/devops.xml" );
        b = b && FileEnsure( "system/dns.xml" );
        b = b && FileEnsure( "cyan.xsd" );

        if ( b == false )
            return 1;

        return 0;
    }


    /// <summary />
    private bool FileEnsure( string skel )
    {
        var xml = _svc.SkeletonGet( skel );
        var path = Path.Combine( _config.Root, skel );

        if ( File.Exists( path ) == true )
        {
            _logger.LogError( "File {File} already exists", skel );
            return false;
        }


        /*
         * 
         */
        Directory.CreateDirectory( Path.GetDirectoryName( path )! );
        File.WriteAllText( path, xml );

        _logger.LogInformation( "File {File} created", skel );

        return true;
    }
}