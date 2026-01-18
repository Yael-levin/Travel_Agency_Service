using Microsoft.AspNetCore.Identity;

namespace Travel_Agency_Service.Helpers
{
    public static class PasswordHelper
    {
        private static PasswordHasher<string> hasher = new PasswordHasher<string>();

        public static string HashPassword(string password)
        {
            return hasher.HashPassword(null, password);
        }

        public static bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            var result = hasher.VerifyHashedPassword(null, hashedPassword, inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
