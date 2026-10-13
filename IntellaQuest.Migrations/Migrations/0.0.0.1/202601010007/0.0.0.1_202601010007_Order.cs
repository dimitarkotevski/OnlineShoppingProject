using System.Collections.Generic;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Baseline migration 0.0.0.1 / 202601010007 - dbo.Orders.
    /// </summary>
    public sealed class _0_0_0_1_202601010007_Order : Migration
    {
        public override string Version => "0.0.0.1";

        public override string Timestamp => "202601010007";

        public override IEnumerable<string> LegacyKeys => new[] { "008_Order.sql" };
    }
}