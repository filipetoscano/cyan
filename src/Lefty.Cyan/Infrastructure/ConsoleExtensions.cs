using System.Text.Json;

namespace Lefty.Cyan;

/// <summary />
public static class ConsoleExtensions
{
    private static readonly JsonSerializerOptions _jso = new JsonSerializerOptions() { WriteIndented = true };


    /// <summary />
    public static string xJson<T>( this T value )
    {
        return JsonSerializer.Serialize( value, _jso );
    }


    /// <summary />
    public static void xDump<T>( this T value )
    {
        Console.WriteLine( xJson( value ) );
    }
}