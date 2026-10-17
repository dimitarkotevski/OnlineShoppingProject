using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace NexCart.Migrations
{
    /// <summary>
    /// Applies and rolls back the ShopDb schema with FluentMigrator.
    ///
    /// <code>
    /// dotnet run --project NexCart.Migrations                 # up   (default)
    /// dotnet run --project NexCart.Migrations -- down         # roll back the last applied migration
    /// dotnet run --project NexCart.Migrations -- list         # show migrations and their status
    /// dotnet run --project NexCart.Migrations -- up "Data Source=.;..."
    /// </code>
    ///
    /// Migrations are plain FluentMigrator classes: each one lives in
    /// <c>Migrations\&lt;version&gt;\&lt;timestamp&gt;-&lt;gitUser&gt;\</c> as
    /// <c>sqlmigration.cs</c> plus <c>up.sql</c>/<c>down.sql</c>.
    ///
    /// Applied migrations are tracked by FluentMigrator itself in <c>dbo.VersionInfo</c>.
    /// When a run finishes the runner prints a summary that lists exactly which
    /// migrations were applied (or rolled back) and whether the run succeeded.
    /// </summary>
    public static class Program
    {
        // Same connection string the app uses (NHibernateContext.cs).
        private const string DefaultConnectionString =
            "Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;";

        public static int Main(string[] args)
        {
            string command = GetCommand(args);
            string connectionString = GetConnectionString(args);

            try
            {
                // Execute.Script(...) resolves its relative path against the current
                // directory, so run from the folder that holds the copied *.sql files.
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);

                EnsureDatabaseExists(connectionString);

                // Snapshot what is already applied so the summary can report exactly
                // which migrations this run changes.
                var appliedBefore = ReadAppliedMigrations(connectionString);

                using (var serviceProvider = CreateServices(connectionString))
                using (var scope = serviceProvider.CreateScope())
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                    switch (command)
                    {
                        case "down":
                            runner.Rollback(1);
                            break;
                        case "list":
                            runner.ListMigrations();
                            break;
                        default:
                            runner.MigrateUp();
                            break;
                    }
                }

                var appliedAfter = ReadAppliedMigrations(connectionString);
                PrintSummary(command, appliedBefore, appliedAfter);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Migration FAILED: {0}", ex.Message);
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
                return value;
            }

            return ReadConfiguredConnectionString() ?? DefaultConnectionString;
        }

        // ------------------------------------------------------------------- runner

        private static ServiceProvider CreateServices(string connectionString)
        {
            return new ServiceCollection()
                // Registers the FluentMigrator services.
                .AddFluentMigratorCore()
                .ConfigureRunner(runner => runner
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    // Every FluentMigrator Migration in this assembly, ordered by id.
                    .ScanIn(typeof(Program).Assembly).For.Migrations())
                // Writes the [....] progress lines to the console.
                .AddLogging(logging => logging.AddFluentMigratorConsole())
                .BuildServiceProvider(false);
        }

        // ------------------------------------------------------------------ summary

        /// <summary>
        /// Prints a clear "what happened" block after a run. <paramref name="before"/>
        /// and <paramref name="after"/> are the applied migrations read from
        /// <c>dbo.VersionInfo</c> (id -> description) before and after the run.
        /// </summary>
        private static void PrintSummary(
            string command,
            IReadOnlyDictionary<long, string> before,
            IReadOnlyDictionary<long, string> after)
        {
            if (command == "list")
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("================================================================");
            Console.WriteLine("Migration result: SUCCESS ({0})", command);
            Console.WriteLine("----------------------------------------------------------------");

            if (command == "down")
            {
                var rolledBack = before.Keys.Except(after.Keys).OrderByDescending(version => version).ToList();
                if (rolledBack.Count == 0)
                {
                    Console.WriteLine("  Nothing to roll back (no migration was applied).");
                }
                else
                {
                    foreach (var version in rolledBack)
                    {
                        Console.WriteLine("  [rolled back] {0}  {1}", version, before[version]);
                    }
                }
            }
            else
            {
                var applied = after.Keys.Except(before.Keys).OrderBy(version => version).ToList();
                if (applied.Count == 0)
                {
                    Console.WriteLine("  Database already up to date - no migrations to apply.");
                }
                else
                {
                    Console.WriteLine("  Applied {0} migration(s):", applied.Count);
                    foreach (var version in applied)
                    {
                        Console.WriteLine("    [done] {0}  {1}", version, after[version]);
                    }
                }
            }

            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("  Migrations applied: {0}", after.Count);
            Console.WriteLine("================================================================");
        }

        // ---------------------------------------------------------------- database

        /// <summary>
        /// Reads the applied migrations from <c>dbo.VersionInfo</c> as id -> description.
        /// Returns an empty map when the history table does not exist yet (nothing applied).
        /// </summary>
        private static IReadOnlyDictionary<long, string> ReadAppliedMigrations(string connectionString)
        {
            var applied = new Dictionary<long, string>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var probe = connection.CreateCommand())
                {
                    probe.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.VersionInfo', 'U') IS NULL THEN 0 ELSE 1 END";
                    if ((int)probe.ExecuteScalar() == 0)
                    {
                        return applied;
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Version], [Description] FROM dbo.VersionInfo";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            applied[reader.GetInt64(0)] = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        }
                    }
                }
            }

            return applied;
        }

        /// <summary>
        /// Creates the target database when it does not exist yet. The CREATE DATABASE
        /// statement has to run against master, outside any FluentMigrator transaction.
        /// </summary>
        private static void EnsureDatabaseExists(string connectionString)
        {
            string databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

            var masterBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
            using (var connection = new SqlConnection(masterBuilder.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
                    command.Parameters.AddWithValue("@name", databaseName);

                    if ((int)command.ExecuteScalar() != 0)
                    {
                        return;
                    }
                }

                Console.WriteLine("Creating database {0}...", databaseName);
                using (var create = connection.CreateCommand())
                {
                    create.CommandText = "CREATE DATABASE [" + databaseName.Replace("]", "]]") + "]";
                    create.ExecuteNonQuery();
                }
            }
        }

        // ---------------------------------------------------------------- settings

        /// <summary>
        /// Reads ConnectionStrings:ShopDb from appsettings.json (next to the assembly),
        /// falling back to the built-in default when it is missing.
        /// </summary>
        private static string ReadConfiguredConnectionString()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (var document = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    JsonElement connectionStrings;
                    JsonElement shopDb;
                    if (document.RootElement.TryGetProperty("ConnectionStrings", out connectionStrings) &&
                        connectionStrings.TryGetProperty("ShopDb", out shopDb) &&
                        shopDb.ValueKind == JsonValueKind.String)
                    {
                        string value = shopDb.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                Console.Error.WriteLine("Could not parse {0}.", path);
            }

            return null;
        }
    }
}
