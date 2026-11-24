namespace PasswordGen.Models;

public class GeneratePasswordRequest
{
    public int Length { get; set; } = 16;
    public bool UseUppercase { get; set; }
    public bool UseLowercase { get; set; }
    public bool UseDigits { get; set; }
    public bool UseSymbols { get; set; }
    public bool ExcludeLookAlike { get; set; }
}

public class GeneratePasswordResponse
{
    public string Password { get; set; } = "";
    public int Length { get; set; }
    public int PoolSize { get; set; }
    public double EntropyBits { get; set; }
    public string Strength { get; set; } = "";
    public DateTime GeneratedAt { get; set; }
    public int BreachCount { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; } = "";
}

public record HistoryItem(string Password, double EntropyBits, string Strength, DateTime GeneratedAt);
