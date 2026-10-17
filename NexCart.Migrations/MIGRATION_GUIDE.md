# Migration Workflow

When adding a new database migration, always create a folder with the format:
`NexCart.Migrations\Migrations\<version>\<timestamp>-dimitarkotevski\`

Inside that folder, you MUST create these 3 files:

1. **up.sql**: The SQL script to apply the change.
2. **down.sql**: The SQL script to revert the change.
3. **sqlmigration.cs**: The FluentMigrator class that links the migration to the SQL files.

## C# Template for sqlmigration.cs
```csharp
using FluentMigrator;

namespace NexCart.Migrations
{
    [Migration(<timestamp>, "<description>")]
    public sealed class Migration_<timestamp> : Migration
    {
        public override void Up()
        {
            Execute.Script(@"Migrations\<version>\<timestamp>-dimitarkotevski\up.sql");
        }

        public override void Down()
        {
            Execute.Script(@"Migrations\<version>\<timestamp>-dimitarkotevski\down.sql");
        }
    }
}
```
