using Prodora.Entitys;

namespace Prodora.Tests;

public class AuditEntryTests
{
    [Fact]
    public void Line_breaks_and_control_characters_become_spaces_so_a_value_cannot_fake_another_log_line() =>
        Assert.Equal("ok  forged line", AuditEntry.Clean("ok\r\nforged\tline", 100));

    [Fact]
    public void Text_longer_than_the_column_is_cut_with_an_ellipsis()
    {
        var cleaned = AuditEntry.Clean(new string('a', 600), AuditEntry.DetailLength);

        Assert.Equal(AuditEntry.DetailLength, cleaned.Length);
        Assert.EndsWith("…", cleaned);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  kısa  ", "kısa")]
    public void Empty_and_padded_values_are_normalised(string? input, string expected) =>
        Assert.Equal(expected, AuditEntry.Clean(input, 50));

    [Fact]
    public void Text_that_fits_is_left_alone() =>
        Assert.Equal("Sipariş #000012", AuditEntry.Clean("Sipariş #000012", AuditEntry.TargetLength));
}
