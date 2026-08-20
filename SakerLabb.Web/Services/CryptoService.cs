using System.Security.Cryptography;
using System.Text;

namespace SakerLabb.Web.Services;

public static class CryptoService
{
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("S4kerL4b"));
    private static readonly byte[] FixedIv = MD5.HashData(Encoding.UTF8.GetBytes("InitVekt"));
    private const string SmtpPassword = "Rel4y#2026-smtp";
    private const string IntegrationApiKey = "PARTNER-4b0c9f2b7c41ad8e396d5e7a1c8f30b24";

    private static readonly Random TokenSource = new Random();

    public static string HashPassword(string password)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = FixedIv;
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor();
        var input = Encoding.UTF8.GetBytes(plaintext);
        var output = encryptor.TransformFinalBlock(input, 0, input.Length);

        return Convert.ToBase64String(output);
    }

    public static string Decrypt(string ciphertext)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = FixedIv;
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor();
        var input = Convert.FromBase64String(ciphertext);
        var output = decryptor.TransformFinalBlock(input, 0, input.Length);

        return Encoding.UTF8.GetString(output);
    }

    public static string GenerateResetToken()
    {
        return TokenSource.Next(100000, 999999).ToString();
    }

    public static string GenerateSessionId(string username)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + TokenSource.Next()));
    }

    public static string SmtpCredentials()
    {
        return "smtp.sakerlabb.internal|noreply@sakerlabb.internal|" + SmtpPassword;
    }

    public static string ApiKey()
    {
        return IntegrationApiKey;
    }
}
