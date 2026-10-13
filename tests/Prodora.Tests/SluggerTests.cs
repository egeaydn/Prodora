using Prodora.WebUI.Seo;

namespace Prodora.Tests;

public class SluggerTests
{
    [Theory]
    [InlineData("Kırmızı Ayakkabı (No:42)", "kirmizi-ayakkabi-no-42")]
    [InlineData("ÇİĞDEM ŞÖLEN ÜRÜNÜ", "cigdem-solen-urunu")]
    [InlineData("  Çok   boşluk -- ve --- tire ", "cok-bosluk-ve-tire")]
    [InlineData("iPhone 15 Pro", "iphone-15-pro")]
    [InlineData("ISPARTA gülü", "isparta-gulu")]
    public void Turkish_names_become_ascii_slugs(string name, string expected) =>
        Assert.Equal(expected, Slugger.Slugify(name));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("%%% ???")]
    public void Names_without_letters_or_digits_get_a_fallback(string? name) =>
        Assert.Equal("urun", Slugger.Slugify(name));

    [Fact]
    public void Slugs_are_limited_in_length_and_never_end_with_a_dash()
    {
        var slug = Slugger.Slugify(new string('a', 58) + " bbbbbb");
        Assert.True(slug.Length <= 60);
        Assert.False(slug.EndsWith('-'));
    }

    [Fact]
    public void Markup_and_path_characters_cannot_survive() =>
        Assert.Equal("script-alert-1-script", Slugger.Slugify("<script>alert(1)</script>/../"));

    [Fact]
    public void Slugging_is_stable() =>
        Assert.Equal(Slugger.Slugify("Çanta"), Slugger.Slugify(Slugger.Slugify("Çanta")));
}
