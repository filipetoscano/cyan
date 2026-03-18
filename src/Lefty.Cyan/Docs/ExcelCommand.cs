using LargeXlsx;
using Lefty.Cyan.Repository;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml;
using XP = LargeXlsx.XlsxPalette;

namespace Lefty.Cyan.Docs;

/// <summary />
[Command( "excel", Description = "Consolidated Excel" )]
public class ExcelCommand
{
    private readonly CyanConfiguration _config;
    private readonly RepositoryService _repo;
    private readonly ILogger<ExcelCommand> _logger;

    /// <summary />
    public ExcelCommand( IOptions<CyanConfiguration> config,
        RepositoryService repo,
        ILogger<ExcelCommand> logger )
    {
        _config = config.Value;
        _repo = repo;
        _logger = logger;
    }


    /// <summary />
    [Option( "-O|--output-dir", CommandOptionType.SingleValue, Description = "Directory where file should be generated, otherwise PWD" )]
    public string? OutputDirectory { get; set; }


    /// <summary />
    public int OnExecute()
    {
        /*
         * 
         */
        var devop = _repo.DevopsGet();
        var projects = devop.Projects.Select( x => x.Name ).OrderBy( x => x ).ToList();


        /*
         * 
         */
        var dir = _repo.DirectoryGet();
        var mgr = _repo.NamespaceManager();

        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            var res = _repo.PersonRbac( p.CompanyCode, p.Name );
            p.Rbac = res.Data;
        }


        /*
         * 
         */
        string dirName;

        if ( this.OutputDirectory == null )
            dirName = Environment.CurrentDirectory;
        else
            dirName = Path.Combine( Environment.CurrentDirectory, this.OutputDirectory );

        Directory.CreateDirectory( dirName );
        var dateTime = DateTime.UtcNow.ToString( "yyyyMMdd-HHmmZ" );
        var fileName = Path.Combine( dirName, $"{_config.DevopsOrganization}-{dateTime}.xlsx" );


        /*
         * 
         */
        using var stream = new FileStream( fileName, FileMode.Create, FileAccess.Write );
        using var xlsxWriter = new XlsxWriter( stream );


        /*
         * Devops RBAC
         */
        BeginDevopsSheet( xlsxWriter, projects );

        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            xlsxWriter
                .BeginRow()
                .Write( p.CompanyCode )
                .Write( p.Name )
                .Write( p.Id )
                .Write( p.Username )
                .Write( p.DateExpiry, XlsxStyles.Date )
                .Write( p.Rbac!.SelectSingleNode( " /c:rbac/c:devops/@access ", mgr )?.Value );

            foreach ( var j in projects )
            {
                var elem = (XmlElement?) p.Rbac!.SelectSingleNode( $" /c:rbac/c:devops/c:project[ @name = '{j}' ]/c:*[ 1 ] ", mgr );
                string v = "";

                if ( elem != null )
                    v = elem.Attributes[ "name" ]!.Value;

                xlsxWriter.Write( v );
            }
        }


        /*
         * Azure RBAC
         */
        BeginAzureSheet( xlsxWriter );

        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            foreach ( var rb in p.Rbac!.SelectNodes( " /c:rbac/c:azure/c:* ", mgr )!.OfType<XmlElement>() )
            {
                xlsxWriter
                    .BeginRow()
                    .Write( p.Username )
                    .Write( rb.LocalName )
                    .Write( rb.Attributes[ "name" ]!.Value )
                    .Write( rb.Attributes[ "role" ]!.Value );
            }
        }


        /*
         * Jump servers
         */
        BeginJumpSheet( xlsxWriter );

        foreach ( var p in dir.SelectMany( x => x.Persons ?? [] ) )
        {
            foreach ( var rb in p.Rbac!.SelectNodes( " /c:rbac/c:jump ", mgr )!.OfType<XmlElement>() )
            {
                xlsxWriter
                    .BeginRow()
                    .Write( p.Username )
                    .Write( rb.Attributes[ "server" ]!.Value );
            }
        }


        return 0;
    }


    /// <summary />
    private void BeginDevopsSheet( XlsxWriter writer, List<string> projects )
    {
        var columns = new List<XlsxColumn>()
        {
            // Green
            XlsxColumn.Formatted( 11 ),
            XlsxColumn.Formatted( 26 ),

            // Orange
            XlsxColumn.Formatted( 11 ),
            XlsxColumn.Formatted( 21 ),
            XlsxColumn.Formatted( 11 ),

            // Blue
            XlsxColumn.Formatted( 11 ),
            XlsxColumn.Formatted( 21 ),
        };

        for ( int i = 0; i < projects.Count() - 1; i++ )
            columns.Add( XlsxColumn.Formatted( 21 ) );


        writer.BeginWorksheet( "Devops", 2, 4, columns: columns );

        writer.BeginRow()
            .Write( "Person", XP.GreenDark )
            .Write( null, XP.GreenDark )

            .Write( "Entra", XP.OrangeDark )
            .Write( null, XP.OrangeDark )
            .Write( null, XP.OrangeDark )

            .Write( "Organization", XP.BlueDark )
            .Write( "Project", XP.BlueDark )
            ;

        for ( int i = 0; i < projects.Count() - 1; i++ )
            writer.Write( "", XP.BlueDark );


        writer.BeginRow()
            .Write( "Company", XP.GreenLight )
            .Write( "Name", XP.GreenLight )

            .Write( "Id", XP.OrangeLight )
            .Write( "Username", XP.OrangeLight )
            .Write( "Expiration", XP.OrangeLight )

            .Write( "Access", XP.BlueLight )
            ;

        foreach ( var p in projects )
            writer.Write( p, XP.BlueLight );

        writer
            .SetAutoFilter( 2, 1, 1, 6 + projects.Count() );
    }


    /// <summary />
    private void BeginAzureSheet( XlsxWriter writer )
    {
        var columns = new List<XlsxColumn>()
        {
            // Green
            XlsxColumn.Formatted( 26 ),

            // Blue
            XlsxColumn.Formatted( 36 ),
            XlsxColumn.Formatted( 36 ),
            XlsxColumn.Formatted( 36 ),
        };

        writer.BeginWorksheet( "Azure", 2, 1, columns: columns );

        writer.BeginRow()
            .Write( "Entra", XP.OrangeDark )
            .Write( "Azure", XP.BlueDark )
            .Write( "", XP.BlueDark )
            .Write( "", XP.BlueDark )
            ;

        writer.BeginRow()
            .Write( "Username", XP.OrangeLight )
            .Write( "Type", XP.BlueLight )
            .Write( "Name", XP.BlueLight )
            .Write( "Role", XP.BlueLight )
            ;

        writer
            .SetAutoFilter( 2, 1, 1, 4 );
    }


    /// <summary />
    private void BeginJumpSheet( XlsxWriter writer )
    {
        var columns = new List<XlsxColumn>()
        {
            // Green
            XlsxColumn.Formatted( 26 ),

            // Blue
            XlsxColumn.Formatted( 36 ),
            XlsxColumn.Formatted( 36 ),
            XlsxColumn.Formatted( 36 ),
        };

        writer.BeginWorksheet( "Jump", 2, 1, columns: columns );

        writer.BeginRow()
            .Write( "Entra", XP.OrangeDark )
            .Write( "Jump", XP.BlueDark )
            ;

        writer.BeginRow()
            .Write( "Username", XP.OrangeLight )
            .Write( "Server", XP.BlueLight )
            ;

        writer
            .SetAutoFilter( 2, 1, 1, 2 );
    }
}