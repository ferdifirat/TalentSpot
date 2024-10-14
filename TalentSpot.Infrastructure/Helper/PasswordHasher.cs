using System.Security.Cryptography;

namespace TalentSpot.Infrastructure.Helper
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32; 
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var salt = algorithm.Salt;
                var key = algorithm.GetBytes(KeySize);

                var hashParts = new byte[SaltSize + KeySize];
                Array.Copy(salt, 0, hashParts, 0, SaltSize);
                Array.Copy(key, 0, hashParts, SaltSize, KeySize);

                return Convert.ToBase64String(hashParts);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);

            var salt = new byte[SaltSize];
            var key = new byte[KeySize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            Array.Copy(hashBytes, SaltSize, key, 0, KeySize);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(KeySize);

                return keyToCheck.SequenceEqual(key);
            }
        }
    }
}
