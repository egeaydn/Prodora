using NSubstitute;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Tests;

public class AddressRulesTests
{
    private static SavedAddress Valid() => new()
    {
        Title = "Ev", FirstName = "Ege", LastName = "Aydın", Phone = "0555 123 45 67", Address = "Örnek Mah. 1. Sk. No:2", City = "İstanbul"
    };

    [Fact]
    public void A_complete_address_is_valid()
    {
        var address = Valid();
        AddressRules.Normalize(address);

        Assert.Null(AddressRules.Validate(address));
    }

    [Theory]
    [InlineData("0555 123 45 67", "05551234567")]
    [InlineData("(0555) 123-45-67", "05551234567")]
    [InlineData("+90 555 123 45 67", "+905551234567")]
    [InlineData("  555.123.4567 ", "5551234567")]
    public void Phone_numbers_keep_only_digits_and_a_leading_plus(string input, string expected)
    {
        var address = Valid(); address.Phone = input;
        AddressRules.Normalize(address);

        Assert.Equal(expected, address.Phone);
    }

    [Fact]
    public void Normalize_trims_every_field()
    {
        var address = new SavedAddress { Title = " Ev ", FirstName = " Ege ", LastName = " Aydın ", Address = " Adres ", City = " Ankara ", Phone = "5551234567" };
        AddressRules.Normalize(address);

        Assert.Equal(("Ev", "Ege", "Aydın", "Adres", "Ankara"), (address.Title, address.FirstName, address.LastName, address.Address, address.City));
    }

    [Theory]
    [InlineData("Title", "")]
    [InlineData("Title", "0123456789012345678901234567890")]
    [InlineData("FirstName", "")]
    [InlineData("LastName", "")]
    [InlineData("Address", "")]
    [InlineData("City", "")]
    [InlineData("Phone", "12345")]
    [InlineData("Phone", "1234567890123456")]
    [InlineData("Phone", "abc")]
    public void Missing_or_out_of_range_fields_are_rejected(string field, string value)
    {
        var address = Valid();
        typeof(SavedAddress).GetProperty(field)!.SetValue(address, value);
        AddressRules.Normalize(address);

        Assert.NotNull(AddressRules.Validate(address));
    }

    [Fact]
    public void Address_length_limits_match_the_checkout_form()
    {
        var address = Valid(); address.Address = new string('a', 251);
        Assert.NotNull(AddressRules.Validate(address));
        address.Address = new string('a', 250);
        Assert.Null(AddressRules.Validate(address));
    }

    [Fact]
    public void Address_book_size_limit_is_ten() => Assert.Equal(10, AddressRules.MaxPerUser);
}

public class AddressManagerTests
{
    private readonly IAddressDal dal = Substitute.For<IAddressDal>();

    [Fact]
    public void Everything_is_scoped_to_the_given_customer()
    {
        var manager = new AddressManager(dal);

        manager.GetAll("user-1");
        manager.Get("user-1", 4);
        manager.SetDefault("user-1", 4);
        manager.Delete("user-1", 4);

        dal.Received(1).GetAll("user-1");
        dal.Received(1).Get("user-1", 4);
        dal.Received(1).SetDefault("user-1", 4);
        dal.Received(1).Delete("user-1", 4);
    }
}
