using System;
using System.IO;
using System.Text.Json;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Applies and rolls back the ShopDb schema with FluentMigrator.
    ///
    /// <code>
    /// dotnet run --project IntellaQuest.Migrations                 # up   (default)
    /// dotnet run --project IntellaQuest.Migrations -- down         # roll back the last applied migration
    /// dotnet run --project IntellaQuest.Migrations -- list         # show migrations and their status
    /// dotnet run --project IntellaQuest.Migrations -- up "Data Source=.;..."
    /// </code>
    ///
    /// Migrations are plain FluentMigrator classes: the original schema is defined
    /// inline in <c>Migrations\*_&lt;id&gt;.cs</c>, and new changes are scaffolded with
    /// <c>new-migration.tt</c> into <c>Migrations\&lt;version&gt;\&lt;id&gt;_&lt;gitUser&gt;\</c>
    /// as a class plus <c>up.sql</c>/<c>down.sql</c>.
    ///
    /// Applied migrations are tracked by FluentMigrator itself in <c>dbo.VersionInfo</c>.
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

                return 0;
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

        // ---------------------------------------------------------------- database

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
