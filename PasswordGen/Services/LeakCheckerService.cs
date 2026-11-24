using System.Security.Cryptography;
using System.Text;

namespace PasswordGen.Services;

public class LeakCheckerService
{
    private readonly HttpClient _httpClient;

    // HttpClient внедряется через конструктор (мы настроим это в Program.cs)
    public LeakCheckerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    
    public async Task<int> CheckPasswordLeakAsync(string password)
    {
        try
        {
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            var prefix = hashString.Substring(0, 5);
            var suffix = hashString.Substring(5);
            var response = await _httpClient.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && parts[0].Equals(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return int.Parse(parts[1]);
                }
            }
            return 0;
        }
        catch
        {
            return -1;
        }
    }
}