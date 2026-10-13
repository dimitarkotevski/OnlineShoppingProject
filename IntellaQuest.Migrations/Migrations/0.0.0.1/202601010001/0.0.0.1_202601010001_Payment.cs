using System.Collections.Generic;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Baseline migration 0.0.0.1 / 202601010001 - dbo.Payments.
    /// </summary>
    public sealed class _0_0_0_1_202601010001_Payment : Migration
    {
        public override string Version => "0.0.0.1";

        public override string Timestamp => "202601010001";

        public override IEnumerable<string> LegacyKeys => new[] { "002_Payment.sql" };
    }
}