using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using PClub.Application.Abstractions;

namespace PClub.Infrastructure.Security
{
    /// <summary>PBKDF2-HMAC-SHA256, соль и параметры хранятся вместе с хешем.</summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        // OWASP Password Storage Cheat Sheet: PBKDF2-HMAC-SHA256, 600 000 iterations (2026-11).
        private const int DefaultIterations = 600_000;

        private const int SaltSize = 16;

        private const int HashSize = 32;

        private const string Algorithm = "pbkdf2-sha256";

        /// <inheritdoc />
        public string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: DefaultIterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashSize);

            return string.Join('$',
                Algorithm,
                DefaultIterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }
        /// <inheritdoc />
        public bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split('$');

            if (parts.Length != 4 || parts[0] != Algorithm)
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations) || iterations <= 0 || iterations > 2_000_000)
            {
                return false;
            }

            byte[] salt;
            byte[] expected;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length != SaltSize || expected.Length != HashSize) return false;

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

    }
}
