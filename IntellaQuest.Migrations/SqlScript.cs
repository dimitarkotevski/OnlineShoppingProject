using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Helpers for working with raw T-SQL scripts.
    /// </summary>
    public static class SqlScript
    {
        /// <summary>
        /// Splits a script on the client-side "GO" batch separator. "GO" is recognised
        /// on a line by itself (case-insensitive); only non-empty batches are returned.
        /// </summary>
        public static IEnumerable<string> SplitBatches(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return Array.Empty<string>();
            }

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
