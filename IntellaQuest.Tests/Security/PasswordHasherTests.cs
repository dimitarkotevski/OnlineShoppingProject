using IntellaQuest.BusinessLogic.Security;
using Xunit;

namespace IntellaQuest.Tests.Security
{
    public class PasswordHasherTests
    {
        private const string Password = "correct horse battery staple";

        [Fact]
        public void Hash_produces_a_unique_value_each_time()
        {
            string first = PasswordHasher.Hash(Password);
            string second = PasswordHasher.Hash(Password);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Hash_then_verify_round_trips()
        {
            string stored = PasswordHasher.Hash(Password);

            Assert.True(PasswordHasher.Verify(Password, stored));
        }

        [Fact]
        public void Verify_rejects_a_wrong_password()
        {
            string stored = PasswordHasher.Hash(Password);

            Assert.False(PasswordHasher.Verify("not the password", stored));
        }

        [Fact]
        public void Verify_rejects_missing_or_empty_stored_values()
        {
            Assert.False(PasswordHasher.Verify(Password, null));
            Assert.False(PasswordHasher.Verify(Password, string.Empty));
        }

        [Fact]
        public void NeedsRehash_is_true_only_for_legacy_and_empty_values()
        {
            Assert.True(PasswordHasher.NeedsRehash(null));
            Assert.True(PasswordHasher.NeedsRehash(string.Empty));
            Assert.True(PasswordHasher.NeedsRehash("some-legacy-aes-blob"));
            Assert.False(PasswordHasher.NeedsRehash(PasswordHasher.Hash(Password)));
        }

        [Fact]
        public void MeetsPolicy_enforces_the_minimum_length()
        {
            Assert.False(PasswordHasher.MeetsPolicy(null));
            Assert.False(PasswordHasher.MeetsPolicy("        "));
            Assert.False(PasswordHasher.MeetsPolicy("short"));
            Assert.True(PasswordHasher.MeetsPolicy("long enough"));
        }
    }
}
