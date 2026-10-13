using System;
using System.Collections.Generic;
using System.IO;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Base class for a folder-based migration.
    ///
    /// A migration lives in its own dated folder under <c>Migrations\</c>:
    ///
    /// <code>
    /// Migrations\20261013120000_AddOrdersTable\
    ///     20261013120000_AddOrdersTable.cs   &lt;- this class (Id + Name)
    ///     up.sql                             &lt;- applied by "up"   (required)
    ///     down.sql                           &lt;- applied by "down" (optional)
    /// </code>
    ///
    /// Scaffold one with:
    /// <code>
    /// .\new-migration.ps1 AddOrdersTable
    /// </code>
    ///
    /// The runner discovers every concrete <see cref="Migration"/> in the assembly,
    /// orders them by <see cref="Id"/> and applies the forward scripts. Applied
    /// migrations are recorded in <c>dbo.__SqlMigrations</c> by <see cref="Key"/>.
    ///
    /// <b>Never change Id/Name once a migration has been applied</b> — that changes
    /// the key and the migration would run a second time.
    /// </summary>
    public abstract class Migration
    {
        /// <summary>
        /// Sortable, unique id. For scaffolded migrations this is the timestamp taken
        /// from the folder name (<c>yyyyMMddHHmmss</c>).
        /// </summary>
        public abstract long Id { get; }

        /// <summary>Short PascalCase name — the suffix of the folder name.</summary>
        public abstract string Name { get; }

        /// <summary>Folder under <c>Migrations\</c> that holds this migration's scripts.</summary>
        public virtual string Folder
        {
            get { return Id + "_" + Name; }
        }

        /// <summary>
        /// Stable key recorded in <c>dbo.__SqlMigrations</c>. Defaults to the folder
        /// name. Do not change it once the migration has been applied.
        /// </summary>
        public virtual string Key
        {
            get { return Folder; }
        }

        /// <summary>Human-readable label used in console output.</summary>
        public string DisplayName
        {
            get { return Folder; }
        }

        /// <summary>Folder the .sql files are copied to next to the built assembly.</summary>
        protected virtual string ScriptsRoot
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations"); }
        }

        /// <summary>SQL batches applied when moving forward (up.sql).</summary>
        public virtual IEnumerable<string> UpBatches()
        {
            return ReadBatches("up.sql");
        }

        /// <summary>SQL batches applied when rolling back (down.sql).</summary>
        public virtual IEnumerable<string> DownBatches()
        {
            return ReadBatches("down.sql");
        }

        /// <summary>True when this migration shipped a non-empty down.sql.</summary>
        public bool HasDown()
        {
            return ReadBatches("down.sql").GetEnumerator().MoveNext();
        }

        protected IEnumerable<string> ReadBatches(string fileName)
        {
            string path = Path.Combine(ScriptsRoot, Folder, fileName);
            if (!File.Exists(path))
            {
                return Array.Empty<string>();
            }

            return SqlScript.SplitBatches(File.ReadAllText(path));
        }
    }
}
