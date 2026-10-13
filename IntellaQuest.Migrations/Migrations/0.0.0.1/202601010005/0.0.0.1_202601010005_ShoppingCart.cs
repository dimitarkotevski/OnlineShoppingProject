using System.Collections.Generic;

namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Baseline migration 0.0.0.1 / 202601010005 - dbo.ShoppingCarts.
    /// </summary>
    public sealed class _0_0_0_1_202601010005_ShoppingCart : Migration
    {
        public override string Version => "0.0.0.1";

        public override string Timestamp => "202601010005";

        public override IEnumerable<string> LegacyKeys => new[] { "006_ShoppingCart.sql" };
    }
}