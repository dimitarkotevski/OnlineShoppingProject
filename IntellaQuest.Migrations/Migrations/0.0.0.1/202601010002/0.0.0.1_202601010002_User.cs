using System.Collections.Generic;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Baseline migration 0.0.0.1 / 202601010002 - dbo.Users.
    /// </summary>
    public sealed class _0_0_0_1_202601010002_User : Migration
    {
        public override string Version => "0.0.0.1";

        public override string Timestamp => "202601010002";

        public override IEnumerable<string> LegacyKeys => new[] { "003_User.sql" };
    }
}