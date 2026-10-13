using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Runs every SQL migration in the Migrations\ folder against ShopDb.
    /// Applied migrations are tracked in dbo.__SqlMigrations so each script runs only once.
    /// </summary>
    public static class Program
    {
        // Same connection string the app uses (NHibernateContext.cs).
        private const string DefaultConnectionString =
            "Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;";

        private const string HistoryTable = "dbo.__SqlMigrations";

        public static int Main(string[] args)
        {
            string connectionString = args.FirstOrDefault() ?? DefaultConnectionString;
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

                    var applied = GetAppliedMigrations(connection);
                    var scripts = Directory.GetFiles(scriptsPath, "*.sql")
                        .Select(Path.GetFileName)
                        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (!scripts.Any())
                    {
                        Console.WriteLine("No migration files found in {0}", scriptsPath);
                        return 1;
                    }

                    int appliedCount = 0;
                    foreach (var script in scripts)
                    {
                        if (applied.Contains(script))
                        {
                            Console.WriteLine("[skip]    {0} (already applied)", script);
                            continue;
                        }

                        string sql = File.ReadAllText(Path.Combine(scriptsPath, script));
                        ExecuteMigration(connection, script, sql);
                        appliedCount++;
                        Console.WriteLine("[applied] {0}", script);
                    }

                    Console.WriteLine();
                    Console.WriteLine("Done. {0} of {1} migration(s) applied.", appliedCount, scripts.Count);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Migration failed: {0}", ex.Message);
                return 1;
            }
        }

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

        private static void ExecuteMigration(SqlConnection connection, string name, string sql)
        {
            using (var transaction = connection.BeginTransaction())
            {
                // "GO" is a client-side batch separator, not T-SQL — split on it.
                foreach (var batch in SplitBatches(sql))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = batch;
                        command.ExecuteNonQuery();
                    }
                }

                using (var record = connection.CreateCommand())
                {
                    record.Transaction = transaction;
                    record.CommandText = "INSERT INTO " + HistoryTable + " (Name) VALUES (@name)";
                    record.Parameters.AddWithValue("@name", name);
                    record.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static IEnumerable<string> SplitBatches(string sql)
        {
            var batches = new List<string>();
            var current = new StringBuilder();

            foreach (var line in sql.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
                {
                    batches.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.AppendLine(line);
                }
            }

            batches.Add(current.ToString());
            return batches.Where(batch => !string.IsNullOrWhiteSpace(batch));
        }
    }
}
