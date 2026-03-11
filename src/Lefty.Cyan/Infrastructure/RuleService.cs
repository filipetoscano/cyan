namespace Lefty.Cyan.Infrastructure;

/// <summary />
public class RuleService
{
    private readonly DateOnly _today;


    /// <summary />
    public RuleService()
    {
        _today = DateOnly.FromDateTime( DateTime.Today );
    }


    /// <summary />
    public bool IsExpired( DateOnly dateExpiry )
    {
        if ( _today > dateExpiry )
            return true;

        return false;
    }
}