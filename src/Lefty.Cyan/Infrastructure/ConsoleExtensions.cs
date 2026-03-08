using System.Text.Json;

namespace Lefty.Cyan;

/// <summary />
public static class ConsoleExtensions
{
    private static readonly JsonSerializerOptions _jso = new JsonSerializerOptions() { WriteIndented = true };


    /// <summary />
    public static void Dump<T>( this T value )
    {
        var json = JsonSerializer.Serialize( value , _jso );
        Console.WriteLine( json );
    }
}