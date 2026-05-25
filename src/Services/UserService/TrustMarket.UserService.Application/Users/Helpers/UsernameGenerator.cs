using System.Text;

namespace TrustMarket.UserService.Application.Users.Helpers;

internal static class UsernameGenerator
{
    private static readonly Dictionary<char, string> UkrToLatin = new()
    {
        ['а']="a",['б']="b",['в']="v",['г']="h",['ґ']="g",
        ['д']="d",['е']="e",['є']="ie",['ж']="zh",['з']="z",
        ['и']="y",['і']="i",['ї']="i",['й']="i",['к']="k",
        ['л']="l",['м']="m",['н']="n",['о']="o",['п']="p",
        ['р']="r",['с']="s",['т']="t",['у']="u",['ф']="f",
        ['х']="kh",['ц']="ts",['ч']="ch",['ш']="sh",['щ']="shch",
        ['ь']="",['ю']="iu",['я']="ia",
        ['ё']="io",['э']="e",['ъ']="",['ы']="y"
    };

    public static string Build(string firstName, string lastName)
    {
        var raw = (firstName + lastName).ToLowerInvariant();
        var sb = new StringBuilder();

        foreach (var ch in raw)
        {
            if (UkrToLatin.TryGetValue(ch, out var lat))
                sb.Append(lat);
            else if (ch >= 'a' && ch <= 'z')
                sb.Append(ch);
            else if (ch >= '0' && ch <= '9')
                sb.Append(ch);
        }

        var base_ = sb.Length > 20 ? sb.ToString()[..20] : sb.ToString();
        if (string.IsNullOrEmpty(base_)) base_ = "user";

        return base_;
    }

    public static string WithSuffix(string baseUsername)
    {
        var digits = Random.Shared.Next(1000, 9999).ToString();
        return $"{baseUsername}{digits}";
    }
}
