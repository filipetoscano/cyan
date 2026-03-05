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
    [Option( "-l|--lax", CommandOptionType.NoValue, Description = "Lax rules: create files if missing" )]
    public bool Relax { get; set; }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        if ( this.Relax == false )
        {
            var b = true;

            b = b && FileEnsure( "system/azure.xml" );
            b = b && FileEnsure( "system/devops.xml" );
            b = b && FileEnsure( "system/dns.xml" );
            b = b && FileEnsure( "system/jump.xml" );
            b = b && FileEnsure( "cyan.xsd" );

            if ( b == false )
                return 1;
        }
        else
        {
            FileEnsure( "system/azure.xml", true );
            FileEnsure( "system/devops.xml", true );
            FileEnsure( "system/dns.xml", true );
            FileEnsure( "system/jump.xml", true );
            FileEnsure( "cyan.xsd", true );
        }

        return 0;
    }


    /// <summary />
    private bool FileEnsure( string skel, bool relaxed = false )
    {
        var xml = _svc.SkeletonGet( skel );
        var path = Path.Combine( _config.Root, skel );

        if ( File.Exists( path ) == true )
        {
            if ( relaxed == true )
                return true;

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