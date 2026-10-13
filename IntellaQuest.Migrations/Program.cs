using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Applies and rolls back SQL migrations against ShopDb.
    ///
    /// <code>
    /// dotnet run --project IntellaQuest.Migrations                 # up   (default)
    /// dotnet run --project IntellaQuest.Migrations -- down         # roll back the last applied migration
    /// dotnet run --project IntellaQuest.Migrations -- list         # show migrations and their status
    /// dotnet run --project IntellaQuest.Migrations -- up "Data Source=.;..."
    /// </code>
    ///
    /// Two kinds of migration are supported and can be mixed:
    ///
    /// <list type="bullet">
    /// <item><b>Versioned migrations</b> —
    /// <c>Migrations\&lt;version&gt;\&lt;timestamp&gt;\{up.sql,down.sql}</c> driven by a
    /// <see cref="Migration"/> class. Create one with <c>new-migration.ps1</c>.</item>
    /// <item><b>Legacy flat scripts</b> — <c>Migrations\001_Role.sql</c> (forward only).
    /// They always run before the versioned migrations.</item>
    /// </list>
    ///
    /// Every applied migration is recorded in <c>dbo.__SqlMigrations</c> by a stable
    /// key, so each runs only once.
    /// </summary>
    public static class Program
    {
        // Same connection string the app uses (NHibernateContext.cs).
        private const string DefaultConnectionString =
            "Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;";

        private const string HistoryTable = "dbo.__SqlMigrations";

        public static int Main(string[] args)
        {
            string command = GetCommand(args);
            string connectionString = GetConnectionString(args);
            string scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations");

            try
            {
                string databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

                // The CREATE DATABASE statement must run against master, outside any transaction.
                var masterBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
                using (var masterConnection = new SqlConnection(masterBuilder.ConnectionString))
                {
                    masterConnection.Open();
                    EnsureDatabaseExists(masterConnection, databaseName);
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    EnsureHistoryTable(connection);

                    var migrations = DiscoverMigrations(scriptsPath);
                    if (migrations.Count == 0)
                    {
                        Console.WriteLine("No migrations found in {0}", scriptsPath);
                        return 1;
                    }

                    switch (command)
                    {
                        case "down":
                            return RunDown(connection, migrations);
                        case "list":
                            return RunList(connection, migrations);
                        default:
                            return RunUp(connection, migrations);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Migration failed: {0}", ex.Message);
                return 1;
            }
        }

        // ---------------------------------------------------------------- arguments

        private static string GetCommand(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg.Trim().ToLowerInvariant())
                {
                    case "up":
                    case "down":
                    case "list":
                        return arg.Trim().ToLowerInvariant();
                }
            }
            return "up";
        }

        private static string GetConnectionString(string[] args)
        {
            foreach (var arg in args)
            {
                string value = arg.Trim();
                if (value.Equals("up", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("down", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return arg;
            }
            return DefaultConnectionString;
        }

        // -------------------------------------------------------------- discovery

        private sealed class MigrationEntry
        {
            public Version Version;
            public string Timestamp;
            public string Key;
            public string DisplayName;
            public bool SupportsDown;
            public Func<IEnumerable<string>> Up;
            public Func<IEnumerable<string>> Down;
        }

        private static List<MigrationEntry> DiscoverMigrations(string scriptsPath)
        {
            var entries = new List<MigrationEntry>();

            // Versioned migrations: every concrete Migration in this assembly.
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract))
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    Console.Error.WriteLine("Skipping {0}: no parameterless constructor.", type.FullName);
                    continue;
                }

                var migration = (Migration)Activator.CreateInstance(type);

                Version version;
                if (!Version.TryParse(migration.Version, out version))
                {
                    throw new InvalidOperationException(
                        "Invalid version '" + migration.Version + "' on " + type.Name +
                        " (expected e.g. 0.0.0.1).");
                }

                entries.Add(new MigrationEntry
                {
                    Version = version,
                    Timestamp = migration.Timestamp,
                    Key = migration.Key,
                    DisplayName = migration.DisplayName,
                    SupportsDown = migration.HasDown(),
                    Up = migration.UpBatches,
                    Down = migration.DownBatches
                });
            }

            // Legacy flat scripts: Migrations\*.sql — run before the versioned ones.
            var legacyVersion = new Version(0, 0, 0, 0);
            if (Directory.Exists(scriptsPath))
            {
                foreach (var file in Directory.GetFiles(scriptsPath, "*.sql"))
                {
                    string captured = file;
                    string name = Path.GetFileName(file);
                    entries.Add(new MigrationEntry
                    {
                        Version = legacyVersion,
                        Timestamp = ParseOrder(name).ToString("D12"),
                        Key = name,
                        DisplayName = name,
                        SupportsDown = false,
                        Up = () => SqlScript.SplitBatches(File.ReadAllText(captured)),
                        Down = () => Array.Empty<string>()
                    });
                }
            }

            var duplicate = entries.GroupBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidOperationException("Duplicate migration key: " + duplicate.Key);
            }

            return entries
                .OrderBy(e => e.Version)
                .ThenBy(e => e.Timestamp, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Leading digits of a file name ("001_Role.sql" -> 1); else long.MaxValue.</summary>
        private static long ParseOrder(string fileName)
        {
            int i = 0;
            while (i < fileName.Length && char.IsDigit(fileName[i]))
            {
                i++;
            }

            long value;
            if (i > 0 && long.TryParse(fileName.Substring(0, i), out value))
            {
                return value;
            }
            return long.MaxValue;
        }

        // ---------------------------------------------------------------- commands

        private static int RunUp(SqlConnection connection, List<MigrationEntry> migrations)
        {
            var applied = GetAppliedMigrations(connection);
            int appliedCount = 0;

            foreach (var migration in migrations)
            {
                if (applied.Contains(migration.Key))
                {
                    Console.WriteLine("[skip]    {0} (already applied)", migration.DisplayName);
                    continue;
                }

                ExecuteForward(connection, migration.Key, migration.Up());
                appliedCount++;
                Console.WriteLine("[applied] {0}", migration.DisplayName);
            }

            Console.WriteLine();
            Console.WriteLine("Done. {0} of {1} migration(s) applied.", appliedCount, migrations.Count);
            return 0;
        }

        private static int RunDown(SqlConnection connection, List<MigrationEntry> migrations)
        {
            var applied = GetAppliedMigrations(connection);
            var last = migrations.LastOrDefault(m => applied.Contains(m.Key));

            if (last == null)
            {
                Console.WriteLine("Nothing to roll back.");
                return 0;
            }

            if (!last.SupportsDown)
            {
                Console.Error.WriteLine(
                    "{0} has no down.sql, so it cannot be rolled back.", last.DisplayName);
                return 1;
            }

            var batches = last.Down().ToList();
            if (batches.Count == 0)
            {
                Console.Error.WriteLine("{0} has an empty down.sql; nothing to roll back.", last.DisplayName);
                return 1;
            }

            ExecuteRollback(connection, last.Key, batches);
            Console.WriteLine("[reverted] {0}", last.DisplayName);
            return 0;
        }

        private static int RunList(SqlConnection connection, List<MigrationEntry> migrations)
        {
            var applied = GetAppliedMigrations(connection);
            foreach (var migration in migrations)
            {
                Console.WriteLine("{0} {1}",
                    applied.Contains(migration.Key) ? "[x]" : "[ ]",
                    migration.DisplayName);
            }
            return 0;
        }

        // ------------------------------------------------------------- execution

        private static void ExecuteForward(SqlConnection connection, string key, IEnumerable<string> batches)
        {
            using (var transaction = connection.BeginTransaction())
            {
                ExecuteBatches(connection, transaction, batches);

                using (var record = connection.CreateCommand())
                {
                    record.Transaction = transaction;
                    record.CommandText = "INSERT INTO " + HistoryTable + " (Name) VALUES (@name)";
                    record.Parameters.AddWithValue("@name", key);
                    record.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static void ExecuteRollback(SqlConnection connection, string key, IEnumerable<string> batches)
        {
            using (var transaction = connection.BeginTransaction())
            {
                ExecuteBatches(connection, transaction, batches);

                using (var record = connection.CreateCommand())
                {
                    record.Transaction = transaction;
                    record.CommandText = "DELETE FROM " + HistoryTable + " WHERE Name = @name";
                    record.Parameters.AddWithValue("@name", key);
                    record.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static void ExecuteBatches(SqlConnection connection, SqlTransaction transaction, IEnumerable<string> batches)
        {
            foreach (var batch in batches)
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = batch;
                    command.ExecuteNonQuery();
                }
            }
        }

        // -------------------------------------------------------------- database

        private static void EnsureDatabaseExists(SqlConnection masterConnection, string databaseName)
        {
            using (var command = masterConnection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
                command.Parameters.AddWithValue("@name", databaseName);

                if ((int)command.ExecuteScalar() == 0)
                {
                    Console.WriteLine("Creating database {0}...", databaseName);
                    using (var create = masterConnection.CreateCommand())
                    {
                        create.CommandText = "CREATE DATABASE [" + databaseName.Replace("]", "]]") + "]";
                        create.ExecuteNonQuery();
                    }
                }
            }
        }

        private static void EnsureHistoryTable(SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
IF OBJECT_ID('dbo.__SqlMigrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.__SqlMigrations (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(500) NOT NULL,
        AppliedOn DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE UNIQUE INDEX UX___SqlMigrations_Name ON dbo.__SqlMigrations (Name);
END";
                command.ExecuteNonQuery();
            }
        }

        private static HashSet<string> GetAppliedMigrations(SqlConnection connection)
        {
            var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Name FROM " + HistoryTable;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        applied.Add(reader.GetString(0));
                    }
                }
            }
            return applied;
        }
    }
}
