using BCrypt.Net;

namespace DevoteeAnusanga.Helper
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
            => BCrypt.HashPassword(password);

        public static bool Verify(string password, string hash)
            => BCrypt.Verify(password, hash);
    }
}
