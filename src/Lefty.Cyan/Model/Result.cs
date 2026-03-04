namespace Lefty.Cyan.Model;

/// <summary />
/// <typeparam name="T"></typeparam>
public class Result<T>
{
    private readonly bool _ok;
    private readonly T? _data;
    private readonly List<Error>? _errors;


    /// <summary />
    public Result( T data )
    {
        _ok = true;
        _data = data;
    }


    /// <summary />
    public Result( string code, string message )
    {
        _ok = false;
        _errors = [ new Error() {
            Code = code,
            Message = message,
        } ];
    }


    /// <summary />
    public Result( Error error )
    {
        _ok = false;
        _errors = [ error ];
    }


    /// <summary />
    public Result( IEnumerable<Error> errors )
    {
        _ok = false;
        _errors = errors.ToList();
    }


    /// <summary />
    public bool IsOk { get => _ok; }

    /// <summary />
    public T Data
    {
        get
        {
            if ( _ok == false )
                throw new InvalidOperationException();

            return _data!;
        }
    }
}