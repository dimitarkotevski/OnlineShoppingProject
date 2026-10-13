using System.Collections.Generic;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Baseline migration 0.0.0.1 / 202601010004 - dbo.Products.
    /// </summary>
    public sealed class _0_0_0_1_202601010004_Product : Migration
    {
        public override string Version => "0.0.0.1";

        public override string Timestamp => "202601010004";

        public override IEnumerable<string> LegacyKeys => new[] { "005_Product.sql" };
    }
}