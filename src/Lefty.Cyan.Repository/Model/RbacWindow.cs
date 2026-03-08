namespace Lefty.Cyan.Repository.Model;

/// <summary />
public class RbacWindow
{
    /// <summary />
    public required DateOnly From { get; set; }

    /// <summary />
    public required DateOnly To { get; set; }


    /// <summary />
    public bool IsPast
    {
        get
        {
            var today = DateOnly.FromDateTime( DateTime.Today );

            return ( this.To < today );
        }
    }
}