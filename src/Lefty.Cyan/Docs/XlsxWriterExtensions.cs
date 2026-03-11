namespace LargeXlsx;

/// <summary />
public static class XlsxWriterExtensions
{
    /// <summary />
    public static XlsxWriter Write( this XlsxWriter writer, DateOnly value, XlsxStyle? style = null, int columnSpan = 1 )
    {
        var dt = value.ToDateTime( TimeOnly.MinValue, DateTimeKind.Local );

        writer.Write( dt, style, columnSpan );

        return writer;
    }


    /// <summary />
    public static XlsxWriter Write( this XlsxWriter writer, DateOnly? value, XlsxStyle? style = null, int columnSpan = 1 )
    {
        if ( value.HasValue == false )
            writer.Write( null, style, columnSpan );
        else
            writer.Write( value.Value, style, columnSpan );

        return writer;
    }


    /// <summary />
    public static XlsxWriter Write( this XlsxWriter writer, int? value, XlsxStyle? style = null, int columnSpan = 1 )
    {
        if ( value.HasValue == false )
            writer.Write( null, style, columnSpan );
        else
            writer.Write( value.Value, style, columnSpan );

        return writer;
    }


    /// <summary />
    public static XlsxWriter Write( this XlsxWriter writer, decimal? value, XlsxStyle? style = null, int columnSpan = 1 )
    {
        if ( value.HasValue == false )
            writer.Write( null, style, columnSpan );
        else
            writer.Write( value.Value, style, columnSpan );

        return writer;
    }


    /// <summary />
    public static XlsxWriter Write<T>( this XlsxWriter writer, T value, XlsxStyle? style = null, int columnSpan = 1 )
        where T : struct, Enum
    {
        writer.Write( value.ToString(), style, columnSpan );

        return writer;
    }


    /// <summary />
    public static XlsxWriter Write<T>( this XlsxWriter writer, T? value, XlsxStyle? style = null, int columnSpan = 1 )
        where T : struct, Enum
    {
        writer.Write( value?.ToString(), style, columnSpan );

        return writer;
    }
}