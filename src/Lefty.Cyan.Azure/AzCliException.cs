namespace Lefty.Cyan.Azure;

/// <summary />
public abstract class AzCliException : ApplicationException
{
    /// <summary />
    public AzCliException( string message )
        : base( message )
    {
    }
}


/// <summary />
public class AzNotFoundException : AzCliException
{
    /// <summary />
    public AzNotFoundException( string message )
        : base( message )
    {
    }
}


/// <summary />
public class AzBadRequestException : AzCliException
{
    /// <summary />
    public AzBadRequestException( string message )
        : base( message )
    {
    }
}


/// <summary />
public class AzGenericException : AzCliException
{
    /// <summary />
    public AzGenericException( string message )
        : base( message )
    {
    }
}