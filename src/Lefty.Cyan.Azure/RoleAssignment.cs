using Lefty.Cyan.Azure.Model;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public async Task<List<RoleAssignment>> RoleAssignmentListAsync( string scope )
    {
        var all = await AzCli<List<PermanentRoleAssignmentRoleAssignment>>( "role", "assignment", "list", "--scope", scope );
        var users = all.Where( x => x.principalType == "User" );

        return users.Select( x => new RoleAssignment()
        {
            Id = x.id,
            PrincipalName = x.principalName,
            Role = x.roleDefinitionName,
            RoleId = x.roleDefinitionId,
            Description = x.description,
        } ).ToList();
    }


    /// <summary />
    public async Task<List<RoleAssignment>> ElligibleRoleAssignmentListAsync( string scope )
    {
        var url = $"https://management.azure.com{scope}/providers/Microsoft.Authorization/roleEligibilityScheduleInstances?api-version=2020-10-01";
        var resp = await AzCli<ElligibleRoleAssignmentListResponse>( "rest", "--method", "GET", "--url", $"\"{url}\"" );

        return resp.value
            .Where( x => x.properties != null )
            .Where( x => x.properties.expandedProperties != null )
            .Where( x => x.properties.memberType == "Direct" )
            .Where( x => x.properties!.expandedProperties!.principal?.type == "Direct" )
            .Where( x => x.properties!.expandedProperties!.roleDefinition?.type == "BuiltInRole" )
            .Select( x =>
            {
                var p = x.properties!.expandedProperties!.principal!;
                var r = x.properties!.expandedProperties!.roleDefinition!;

                return new RoleAssignment()
                {
                    Id = x.id,
                    PrincipalName = p.userPrincipalName ?? throw new InvalidOperationException( "missing p.userPrincipalName" ),
                    Role = r.displayName ?? throw new InvalidOperationException( "missing r.displayName" ),
                    RoleId = r.id ?? throw new InvalidOperationException( "missing r.id" ),
                    Description = "cyan|",
                };
            } ).ToList();
    }
}