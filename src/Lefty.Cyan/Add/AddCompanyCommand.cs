using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Cyan.Add;

/// <summary />
[Command( "company", Description = "Adds a company" )]
public class AddCompanyCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _svc;
    private readonly ILogger<AddCompanyCommand> _logger;

    /// <summary />
    public AddCompanyCommand( IOptions<CyanConfiguration> config,
        RepositoryService svc,
        ILogger<AddCompanyCommand> logger )
    {
        _config = config.Value;
        _svc = svc;
        _logger = logger;
    }


    /// <summary />
    [Argument( 0, Description = "Company code" )]
    [Required]
    public string? CompanyCode { get; set; }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        var xml = _svc.SkeletonGet( "people/company/index.xml" );
        var file = $"people/{this.CompanyCode}/index.xml";
        var path = Path.Combine( _config.Root, file );

        if ( File.Exists( path ) == true )
        {
            _logger.LogError( "File {File} already exists", file );
            return 1;
        }


        /*
         * 
         */
        Directory.CreateDirectory( Path.GetDirectoryName( path )! );
        File.WriteAllText( path, xml );

        _logger.LogInformation( "File {File} created", file );

        return 0;
    }
}