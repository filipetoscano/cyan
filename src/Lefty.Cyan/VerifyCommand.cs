using Lefty.Cyan.Azure;
using Lefty.Cyan.Repository;
using Lefty.Cyan.Repository.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml;

namespace Lefty.Cyan;

/// <summary />
[Command( "verify", Description = "Verify if repository objects exist" )]
public class VerifyCommand
{
    private readonly CyanConfiguration _config;
    private readonly AzService _az;
    private readonly RepositoryService _repo;
    private readonly ILogger<VerifyCommand> _logger;


    /// <summary />
    public VerifyCommand( IOptions<CyanConfiguration> config,
        RepositoryService repo,
        AzService az,
        ILogger<VerifyCommand> logger )
    {
        _config = config.Value;
        _az = az;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        await Task.Yield();

        var hasErrors = false;


        /*
         * Check if EntraID users exist
         */
        var dir = new DirectoryInfo( Path.Combine( _config.Root, "people" ) );
        var people = new List<Person>();

        foreach ( var corpDir in dir.GetDirectories() )
        {
            foreach ( var personDir in corpDir.GetDirectories() )
            {
                var pfile = $"people/{corpDir.Name}/{personDir.Name}/index.xml";
                var pdoc = LoadXml( pfile );

                if ( pdoc.IsOk == false )
                    continue;

                var p = _repo.ToPerson( corpDir.Name, pdoc.Data );
                people.Add( p );
            }
        }

        foreach ( var p in people )
        {
            if ( p.Username == null )
                continue;

            if ( p.IsEnabled == false )
                continue;

            if ( p.PrincipalName == null )
                continue;

            try
            {
                var u = await _az.EntraUserGetAsync( p.PrincipalName );
                _logger.LogInformation( "{PrincipalName} = {Id} with {Email}", u.UserPrincipalName, u.Id, u.Email );
            }
            catch ( AzNotFoundException )
            {
                _logger.LogError( "User {PrincipalName} does not exist", p.PrincipalName );
                hasErrors = true;
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Failed to check if user {PrincipalName} exits", p.PrincipalName );
                hasErrors = true;
            }
        }


        /*
         * Check if Azure resources exist
         */
        var azure = LoadXml( "system/azure.xml" );

        // TODO: check


        /*
         * 
         */
        if ( hasErrors == true )
            return 1;

        return 0;
    }


    /// <summary />
    private Result<XmlDocument> LoadXml( string file )
    {
        var path = Path.Combine( _config.Root, file );
        _logger.LogDebug( "{File}: Load", file );


        /*
         * 
         */
        var xml = new XmlDocument();

        try
        {
            xml.Load( path );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "{File}: Failed to load XML", file );
            return new Result<XmlDocument>( "E002", $"File {file} failed to load" );
        }

        return new Result<XmlDocument>( xml );
    }
}