using System.Text;

namespace Prodora.WebUI.Seo;

public static class Slugger
{
    private const int MaxLength = 60;

    /// <summary>"Kırmızı Ayakkabı (No:42)" becomes "kirmizi-ayakkabi-no-42". Returns "urun" when nothing usable is left.</summary>
    public static string Slugify(string? text)
    {
        var builder = new StringBuilder();
        var pendingDash = false;
        foreach (var character in text ?? "")
        {
            var mapped = character switch
            {
                'ç' or 'Ç' => 'c', 'ğ' or 'Ğ' => 'g', 'ı' or 'İ' or 'I' => 'i', 'ö' or 'Ö' => 'o', 'ş' or 'Ş' => 's', 'ü' or 'Ü' => 'u',
                _ => char.ToLowerInvariant(character)
            };
            if (char.IsAsciiLetterOrDigit(mapped))
            {
                if (pendingDash && builder.Length > 0) builder.Append('-');
                pendingDash = false;
                builder.Append(mapped);
            }
            else pendingDash = true;
        }
        var slug = builder.Length > MaxLength ? builder.ToString(0, MaxLength).TrimEnd('-') : builder.ToString();
        return slug.Length == 0 ? "urun" : slug;
    }
}
