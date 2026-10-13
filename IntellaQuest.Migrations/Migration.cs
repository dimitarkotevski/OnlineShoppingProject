using System;
using System.Collections.Generic;
using System.IO;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Base class for a versioned, dated migration.
    ///
    /// A migration lives in its own folder under <c>Migrations\</c>, nested by
    /// version and then by timestamp:
    ///
    /// <code>
    /// Migrations\
    ///     0.0.0.1\
    ///         202610122030\                 (2026-10-12 20:30)
    ///             0.0.0.1_202610122030.cs    &lt;- this class
    ///             up.sql                     &lt;- CREATE (applied by "up")
    ///             down.sql                   &lt;- REMOVE (applied by "down")
    /// </code>
    ///
    /// Scaffold one with:
    /// <code>
    /// .\new-migration.ps1 -Version 0.0.0.1 -Name AddOrdersTable
    /// </code>
    ///
    /// The runner discovers every concrete <see cref="Migration"/> in the assembly,
    /// orders them by <see cref="Version"/> and then <see cref="Timestamp"/>, and
    /// applies the forward scripts. Applied migrations are recorded in
    /// <c>dbo.__SqlMigrations</c> by <see cref="Key"/>.
    ///
    /// <b>Never change Version/Timestamp once a migration has been applied</b> — that
    /// changes the key and the migration would run a second time.
    /// </summary>
    public abstract class Migration
    {
        /// <summary>Version folder this migration belongs to, e.g. <c>0.0.0.1</c>.</summary>
        public abstract string Version { get; }

        /// <summary>Timestamp folder, <c>yyyyMMddHHmm</c>, e.g. <c>202610122030</c>.</summary>
        public abstract string Timestamp { get; }

        /// <summary>Folder under <c>Migrations\</c>, <c>&lt;Version&gt;\&lt;Timestamp&gt;</c>.</summary>
        public virtual string Folder
        {
            get { return Path.Combine(Version, Timestamp); }
        }

        /// <summary>
        /// Stable key recorded in <c>dbo.__SqlMigrations</c>. Defaults to
        /// <c>&lt;Version&gt;/&lt;Timestamp&gt;</c>. Do not change it once applied.
        /// </summary>
        public virtual string Key
        {
            get { return Version + "/" + Timestamp; }
        }

        /// <summary>
        /// Older keys this migration may already be recorded under — for example the
        /// flat file name it had before it was moved into a version folder. The runner
        /// treats any of these as "already applied" and rewrites the history row onto
        /// the canonical <see cref="Key"/>, so scripts converted from the old layout
        /// are never run twice and keys normalise automatically.
        /// </summary>
        public virtual IEnumerable<string> LegacyKeys
        {
            get { return Array.Empty<string>(); }
        }

        /// <summary>Human-readable label used in console output.</summary>
        public string DisplayName
        {
            get { return Key; }
        }

        /// <summary>Folder the .sql files are copied to next to the built assembly.</summary>
        protected virtual string ScriptsRoot
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations"); }
        }

        /// <summary>SQL batches that CREATE the change (up.sql).</summary>
        public virtual IEnumerable<string> UpBatches()
        {
            return ReadBatches("up.sql");
        }

        /// <summary>SQL batches that REMOVE the change (down.sql).</summary>
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
