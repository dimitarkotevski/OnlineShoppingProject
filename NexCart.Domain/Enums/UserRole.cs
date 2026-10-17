namespace NexCart.Domain.Enums
{
    /// <summary>
    /// The built-in roles. Persisted as an INT in dbo.Users.Role (constrained by CK_Users_Role)
    /// and carried in the JWT role claim by name. The numeric values are stored in the database,
    /// so they must never be reordered or reused.
    /// </summary>
    public enum UserRole
    {
        /// <summary>Full-access administrator role.</summary>
        Admin = 1,

        /// <summary>Least-privileged role assigned to every registered customer.</summary>
        User = 2
    }
}
