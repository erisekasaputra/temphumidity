using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Text.Json; 

namespace StreamGate.Worker.SeedWorks;

public static class LicenseValidator
{
    private const string secretKey = "000328Eris@SriWahyuniSulistiyowati@2025"; // Replace with your actual secret key
    private static readonly HttpClient client = new();
    private static readonly JsonSerializerOptions jsonConvertOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<bool> IsValid()
    {
        try
        {
            string url = "http://host.docker.internal:15994/hwd";
            var res = await client.GetStringAsync(url);

            var obj = JsonSerializer.Deserialize<LicenseResponse>(res, jsonConvertOptions);
            if (obj is null || string.IsNullOrWhiteSpace(obj.HardwareKey))
            {
                Console.WriteLine("License response is null or empty.");
                return false;
            }



            string licensePath = "license.key";
            if (!File.Exists(licensePath))
            {
                Console.WriteLine("License file does not exist.");
                return false;
            }

            string storedLicense = File.ReadAllText(licensePath).Trim();
            string license = GenerateLicense(obj.HardwareKey, secretKey);

            if (storedLicense != license)
            {
                Console.WriteLine("License is invalid.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating license: {ex.Message}");
            return false;
        }
    }
    
    
    private static string GenerateLicense(string data, string salt)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(data + salt));
        return Convert.ToHexString(hash);
    }
} 

public record LicenseResponse(string HardwareKey);