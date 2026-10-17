using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Hardens dbo.Roles and dbo.Users.RoleId:
    /// adds the canonical NormalizedName column (+ unique index), seeds the built-in
    /// Admin/User roles and makes Users.RoleId a required, defaulted column.
    /// The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261017170000000, "Harden roles: canonical names, seed Admin/User, require Users.RoleId")]
    public sealed class Migration_20261017170000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017170000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017170000000-dimitarkotevski\down.sql");
        }
    }
}
