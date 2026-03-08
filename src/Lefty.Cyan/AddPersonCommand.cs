using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Cyan;

/// <summary />
[Command( "person", Description = "Adds a person" )]
public class AddPersonCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _svc;
    private readonly ILogger<AddPersonCommand> _logger;

    /// <summary />
    public AddPersonCommand( IOptions<CyanConfiguration> config,
        RepositoryService svc,
        ILogger<AddPersonCommand> logger )
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
    [Argument( 1, Description = "Person name" )]
    [Required]
    public string? PersonName { get; set; }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        /*
         * 
         */
        var corpDir = Path.Combine( _config.Root, $"people/{this.CompanyCode}" );

        if ( Directory.Exists( corpDir ) == false )
        {
            _logger.LogError( "Company {CompanyCode} does not exist", this.CompanyCode );
            return 1;
        }


        /*
         * 
         */
        var xml1 = _svc.SkeletonGet( "people/company/person/index.xml" );
        var xml2 = _svc.SkeletonGet( "people/company/person/rbac.xml" );


        /*
         * 
         */
        var expiration = new DateOnly( DateTime.Today.Year, 12, 31 );

        xml1 = xml1
            .Replace( "{PersonName}", this.PersonName )
            .Replace( "{Expiration}", expiration.ToString( "yyyy-MM-dd" ) );


        /*
         * 
         */
        var file1 = $"people/{this.CompanyCode}/{this.PersonName}/index.xml";
        var file2 = $"people/{this.CompanyCode}/{this.PersonName}/rbac.xml";

        var path1 = Path.Combine( _config.Root, file1 );
        var path2 = Path.Combine( _config.Root, file2 );


        /*
         * 
         */
        Directory.CreateDirectory( Path.GetDirectoryName( path1 )! );

        if ( File.Exists( path1 ) == false )
        {
            File.WriteAllText( path1, xml1 );
            _logger.LogInformation( "Person file {File} created", file1 );
        }

        if ( File.Exists( path2 ) == false )
        {
            File.WriteAllText( path2, xml2 );
            _logger.LogInformation( "RBAC file {File} created", file2 );
        }

        return 0;
    }
}