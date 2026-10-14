using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;

namespace IntellaQuest.BusinessLogic.Security
{
    /// <summary>
    /// LEGACY password protection: reversible AES with a fixed key/IV.
    ///
    /// This is kept only so that accounts created before the move to
    /// <see cref="PasswordHasher"/> can be verified once and transparently upgraded on login.
    /// It must not be used to store new passwords (reversible encryption means anyone with the
    /// key can recover every password).
    /// </summary>
    public static class PasswordEncryption
    {
        public static string Encryption(string plainText)
        {
            string encryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(plainText);
            string cryptedPassword;
            using (Aes encryptor = Aes.Create())
            {
#pragma warning disable SYSLIB0060 // legacy scheme: keep the exact algorithm/derivation for verification
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(
                    encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }
                );
#pragma warning restore SYSLIB0060
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    cryptedPassword = Convert.ToBase64String(ms.ToArray());
                }
            }

            return cryptedPassword;
        }
    }
}
