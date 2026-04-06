using UnityEngine;
using System.Text;
using System.Security.Cryptography;

public static class SecurePlayerPrefs
{
    private static string secretKey = "MySecretKey123"; // change this per project!

    // Save a secure int
    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        string hash = ComputeHash(key, value);
        PlayerPrefs.SetString(key + "_hash", hash);
        PlayerPrefs.Save();
    }

    // Read a secure int
    public static int GetInt(string key, int defaultValue = 0)
    {
        int value = PlayerPrefs.GetInt(key, defaultValue);
        string storedHash = PlayerPrefs.GetString(key + "_hash", "");
        string expectedHash = ComputeHash(key, value);

        if (storedHash != expectedHash)
        {
            Debug.LogWarning($"PlayerPrefs tampering detected for key: {key}");
            return defaultValue;
        }

        return value;
    }

    // Compute a SHA256 hash of key + value + secret
    private static string ComputeHash(string key, int value)
    {
        string raw = key + value.ToString() + secretKey;
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(raw);
            byte[] hashBytes = sha.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}