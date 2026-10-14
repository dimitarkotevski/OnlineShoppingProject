namespace IntellaQuest.BusinessLogic.Security
{
    /// <summary>
    /// JWT configuration bound from the <c>Jwt</c> configuration section. The signing
    /// <see cref="Key"/> is supplied through configuration / user secrets / environment
    /// variables and is never committed to source control.
    /// </summary>
    public class JwtSettings
    {
        public string Key { get; set; }

        public string Issuer { get; set; } = "IntellaQuest";

        public string Audience { get; set; } = "IntellaQuest";

        /// <summary>Token lifetime in days (defaults to 1).</summary>
        public int ExpiryDays { get; set; } = 1;
    }
}
