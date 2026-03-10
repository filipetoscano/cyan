using Lefty.Cyan.Azure;
using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Cyan;

/// <summary />
[Command( "from", Description = "Adds a person based on EntraId settings" )]
public class AddFromCommand
{
    private readonly CyanConfiguration _config;
    private readonly AzService _az;
    private readonly RepositoryService _repo;
    private readonly ILogger<AddFromCommand> _logger;

    /// <summary />
    public AddFromCommand( IOptions<CyanConfiguration> config,
        AzService az,
        RepositoryService repo,
        ILogger<AddFromCommand> logger )
    {
        _config = config.Value;
        _az = az;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    [Argument( 0, Description = "Username (upn without Entra domain)" )]
    [Required]
    public string? Username { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        /*
         * 
         */
        var u = await _az.EntraUserGetAsync( this.Username + _config.EntraDomain );


        /*
         * 
         */
        var uname = this.Username!.ToLowerInvariant();
        var ix = uname.IndexOf( '-' );
        var companyCode = uname.Substring( 0, ix );


        /*
         * 
         */
        var corpDir = Path.Combine( _config.Root, $"people/{companyCode}" );

        if ( Directory.Exists( corpDir ) == false )
        {
            _logger.LogError( "Company {CompanyCode} does not exist", companyCode );
            return 1;
        }


        /*
         * 
         */
        var xml1 = _repo.SkeletonGet( "people/company/person/index2.xml" );
        var xml2 = _repo.SkeletonGet( "people/company/person/rbac.xml" );


        /*
         * 
         */
        var expiration = new DateOnly( DateTime.Today.Year, 12, 31 );

        xml1 = xml1
            .Replace( "{Username}", uname )
            .Replace( "{PersonName}", u.DisplayName )
            .Replace( "{Expiration}", expiration.ToString( "yyyy-MM-dd" ) );


        /*
         * 
         */
        var file1 = $"people/{companyCode}/{u.DisplayName}/index.xml";
        var file2 = $"people/{companyCode}/{u.DisplayName}/rbac.xml";

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