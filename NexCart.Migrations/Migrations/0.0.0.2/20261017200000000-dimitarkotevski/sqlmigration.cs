using FluentMigrator;

namespace NexCart.Migrations
{
    /// <summary>
    /// Replaces the dbo.Roles lookup table + dbo.Users.RoleId foreign key with an INT
    /// dbo.Users.Role column (Admin = 1, User = 2) that backs the NexCart.Domain.Enums.UserRole
    /// enum. The SQL lives in up.sql / down.sql next to this file.
    /// </summary>
    [Migration(20261017200000000, "Replace Roles lookup with INT Users.Role enum")]
    public sealed class Migration_20261017200000000 : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017200000000-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\0.0.0.2\20261017200000000-dimitarkotevski\down.sql");
        }
    }
}
