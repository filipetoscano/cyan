using Lefty.Cyan.Azure.DevOps;

namespace Lefty.Cyan.Azure;

public partial class AzService
{
    /// <summary />
    public async Task<Organization> MembershipAsync()
    {
        /*
         * 
         */
        var q1 = await DevOps<ProjectListResponse>( "devops", "project", "list" );

        var org = new Organization();
        org.Projects = q1.Projects;


        /*
         * Teams
         * When pulling security groups (in the next section), the membership results
         * mix groups and teams -- without a way to tell them apart.
         * 
         * As such, on a per project basis, pull the list of teams for that project.
         * This list is then checked to populate 
         */
        var teams = new Dictionary<string, List<string>>();

        foreach ( var p in org.Projects )
        {
            var q2 = await DevOps<List<TeamEx>>( "devops", "team", "list", "--project", p.Name );
            teams.Add( p.Name, q2.Select( x => x.Name ).ToList() );
        }


        /*
         * (Security) Groups
         */
        foreach ( var p in org.Projects )
        {
            var q3 = await DevOps<SecurityGroup>(
                "devops", "security", "group", "list",
                "--scope", "project", "--project", p.Name );

            p.Groups = new List<Group>();
            p.Teams = new List<Team>();

            foreach ( var sg in q3.Groups )
            {
                var q4 = await DevOps<Dictionary<string, GroupMembership>>(
                    "devops", "security", "group", "membership", "list",
                    "--id", sg.Descriptor );

                // Team
                if ( teams[ p.Name ].Contains( sg.DisplayName ) == true )
                {
                    var t = new Team()
                    {
                        Descriptor = sg.Descriptor,
                        DisplayName = sg.DisplayName,
                        Description = sg.Description,
                    };

                    t.Memberships = q4.Values
                        .Where( x => x.SubjectKind == "user" )
                        .Select( x => new TeamMember()
                        {
                            TeamDescriptor = sg.Descriptor,
                            UserDescriptor = x.Descriptor,
                        } ).ToList();

                    p.Teams.Add( t );
                }
                else
                {
                    var g = new Group()
                    {
                        Descriptor = sg.Descriptor,
                        DisplayName = sg.DisplayName,
                        Description = sg.Description,
                    };

                    g.Memberships = q4.Values
                        .Where( x => x.SubjectKind == "user" )
                        .Select( x => new GroupMember()
                        {
                            GroupDescriptor = sg.Descriptor,
                            UserDescriptor = x.Descriptor,
                        } ).ToList();

                    p.Groups.Add( g );
                }
            }
        }


        /*
         * 
         */
        var q5 = await DevOps<MemberListResponse>( "devops", "user", "list" );
        org.Members = q5.Members;


        /*
         * 
         */
        foreach ( var p in org.Projects )
        {
            if ( p.Groups != null  )
            {
                foreach ( var j in p.Groups )
                {
                    if ( j.Memberships == null )
                        continue;

                    j.Members = new List<string>();

                    foreach ( var m in j.Memberships )
                    {
                        var u = org.Members.SingleOrDefault( x => x.User.Descriptor == m.UserDescriptor );

                        if ( u == null )
                            continue;

                        j.Members.Add( u.User.PrincipalName.ToLowerInvariant() );
                    }
                }
            }

            if ( p.Teams != null )
            {
                foreach ( var j in p.Teams )
                {
                    if ( j.Memberships == null )
                        continue;

                    j.Members = new List<string>();

                    foreach ( var m in j.Memberships )
                    {
                        var u = org.Members.SingleOrDefault( x => x.User.Descriptor == m.UserDescriptor );

                        if ( u == null )
                            continue;

                        j.Members.Add( u.User.PrincipalName.ToLowerInvariant() );
                    }
                }
            }
        }


        return org;
    }
}