namespace Krafter.Extensions;

public static class StringExtensions
{
    public static string FirstLetterToLower(this string str)
    {
        return string.IsNullOrEmpty(str) || char.IsLower(str[0]) ? str : char.ToLower(str[0]) + str.Substring(1);
    }
}