using System.Drawing;
using System.Globalization;

namespace LargeXlsx;

/// <summary />
public class XlsxPalette
{
    /// <summary />
    public static readonly XlsxStyle GreenDark = FromHex( "#a9d08e" );

    /// <summary />
    public static readonly XlsxStyle GreenLight = FromHex( "#e2efda" );


    /// <summary />
    public static readonly XlsxStyle OrangeDark = FromHex( "#f4b084" );

    /// <summary />
    public static readonly XlsxStyle OrangeLight = FromHex( "#fce4d6" );


    /// <summary />
    public static readonly XlsxStyle PurpleDark = FromHex( "#fd8bf8" );

    /// <summary />
    public static readonly XlsxStyle PurpleLight = FromHex( "#fecefc" );


    /// <summary />
    public static readonly XlsxStyle BlueDark = FromHex( "#9bc2e6" );

    /// <summary />
    public static readonly XlsxStyle BlueLight = FromHex( "#ddebf7" );


    /// <summary />
    public static readonly XlsxStyle SteelDark = FromHex( "#acb9ca" );

    /// <summary />
    public static readonly XlsxStyle SteelLight = FromHex( "#d6dce4" );


    /// <summary />
    public static readonly XlsxStyle CyanDark = FromHex( "#c9ffff" );

    /// <summary />
    public static readonly XlsxStyle CyanLight = FromHex( "#efffff" );


    /// <summary />
    public static readonly XlsxStyle GoldDark = FromHex( "#ffd966" );

    /// <summary />
    public static readonly XlsxStyle GoldLight = FromHex( "#fff2cc" );



    /// <summary />
    public static XlsxStyle FromHex( string hex )
    {
        return new XlsxStyle(
            XlsxFont.Default,
            new XlsxFill( AsColor( hex ), XlsxFill.Pattern.Solid ),
            XlsxBorder.None,
            XlsxNumberFormat.General,
            XlsxAlignment.Default );
    }


    /// <summary />
    public static Color AsColor( string hex )
    {
        if ( hex.Length != 7 )
            throw new ArgumentOutOfRangeException( nameof( hex ), "Must be 7 chars long" );

        if ( hex[ 0 ] != '#' )
            throw new ArgumentOutOfRangeException( nameof( hex ), "Must start with #" );

        int rr = int.Parse( hex[ 1..3 ], NumberStyles.HexNumber );
        int gg = int.Parse( hex[ 3..5 ], NumberStyles.HexNumber );
        int bb = int.Parse( hex[ 5..7 ], NumberStyles.HexNumber );

        return Color.FromArgb( rr, gg, bb );
    }
}