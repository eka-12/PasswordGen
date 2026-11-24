using System.Security.Cryptography;
using System.Text;
using PasswordGen.Models;

namespace PasswordGen.Services;

public static class PasswordService
{
    public const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Lower = "abcdefghijklmnopqrstuvwxyz";
    public const string Digits = "0123456789";
    public const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?/";
    public const string LookAlike = "O0oIl1|";

    public static string BuildPool(bool upper, bool lower, bool digits, bool symbols, bool excludeLookAlike)
    {
        var sb = new StringBuilder();
        if (upper) sb.Append(Upper);
        if (lower) sb.Append(Lower);
        if (digits) sb.Append(Digits);
        if (symbols) sb.Append(Symbols);

        string pool = sb.ToString();
        if (excludeLookAlike)
            pool = new string(pool.Where(c => !LookAlike.Contains(c)).ToArray());
        return pool;
    }

    public static List<string> SelectedClasses(GeneratePasswordRequest r)
    {
        var classes = new List<string>();
        if (r.UseUppercase) classes.Add(Filter(Upper, r.ExcludeLookAlike));
        if (r.UseLowercase) classes.Add(Filter(Lower, r.ExcludeLookAlike));
        if (r.UseDigits) classes.Add(Filter(Digits, r.ExcludeLookAlike));
        if (r.UseSymbols) classes.Add(Filter(Symbols, r.ExcludeLookAlike));
        return classes.Where(c => c.Length > 0).ToList();
    }

    private static string Filter(string s, bool exclude) =>
        exclude ? new string(s.Where(c => !LookAlike.Contains(c)).ToArray()) : s;

    public static string Generate(int length, string pool, IReadOnlyList<string> requiredClasses)
    {
        var chars = new List<char>(length);
        foreach (var cls in requiredClasses)
            chars.Add(cls[RandomNumberGenerator.GetInt32(cls.Length)]);

        while (chars.Count < length)
            chars.Add(pool[RandomNumberGenerator.GetInt32(pool.Length)]);

        for (int i = chars.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars.ToArray());
    }

    public static double EntropyBits(int length, int poolSize) =>
        poolSize <= 1 ? 0 : length * Math.Log2(poolSize);

    public static string Strength(double bits) => bits switch
    {
        < 40 => "Слабый",
        < 70 => "Средний",
        _ => "Сильный"
    };
}