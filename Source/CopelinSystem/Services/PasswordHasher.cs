using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CopelinSystem.Services
{
    public class PasswordHasher
    {
        // Based on OWASP recommendations
        private const int SaltSize = 128 / 8; // 128 bit
        private const int KeySize = 256 / 8;  // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool VerifyPassword(string passwordHash, string password)
        {
            if (string.IsNullOrEmpty(passwordHash))
            {
                // No password hash stored, verify failed
                return false;
            }

            var elements = passwordHash.Split(Delimiter);
            if (elements.Length != 2)
            {
                // Invalid hash format
                return false;
            }

            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}
