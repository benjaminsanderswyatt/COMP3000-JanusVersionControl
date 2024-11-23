using System.Security.Cryptography;

namespace backend.Auth
{
    public class PasswordSecurity
    {
        public static bool VerifyPassword(string password, string hash, byte[] salt)
        {
            string newHash = ComputeHash(password, salt);

            return hash == newHash;
        }


        public static (string hashedPassword, byte[] salt) HashSaltPassword(string password)
        {
            byte[] salt = GenerateSalt();

            string hashedPassword = ComputeHash(password, salt);


            return (hashedPassword, salt);
        }


        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16]; // 128 bit salt

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        private static string ComputeHash(string password, byte[] salt)
        {
            const int iterations = 600000; // Num PBKDF2 iterations
            const int hashLength = 32; // 256 bit hash

            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = rfc2898.GetBytes(hashLength);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
