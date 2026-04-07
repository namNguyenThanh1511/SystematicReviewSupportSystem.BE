using System.Security.Cryptography;

namespace SRSS.IAM.Services.Utils
{
    public static class HashHelper
    {
        public static string ComputeSha256Hash(Stream stream)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}