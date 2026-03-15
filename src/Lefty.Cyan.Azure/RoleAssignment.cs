namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public async Task<List<RoleAssignment>> RoleAssignmentListAsync( string scope )
    {
        var all = await AzCli<List<RoleAssignment>>( "role", "assignment", "list", "--scope", scope );
        var users = all.Where( x => x.PrincipalType == "User" );

        return users.ToList();
    }
}