using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers;

[Authorize]
public class AddressesController(IAddressServices addresses, UserManager<ApplicationUser> users) : Controller
{
    private string UserId => users.GetUserId(User) ?? throw new InvalidOperationException("Oturum bulunamadı.");

    [HttpGet]
    public IActionResult Index() => View(addresses.GetAll(UserId));

    [HttpPost]
    public IActionResult Create(string? title, string? firstName, string? lastName, string? phone, string? address, string? city, bool makeDefault)
    {
        try
        {
            var saved = addresses.Add(UserId, new SavedAddress
            {
                Title = title ?? "", FirstName = firstName ?? "", LastName = lastName ?? "", Phone = phone ?? "",
                Address = address ?? "", City = city ?? ""
            });
            if (makeDefault && !saved.IsDefault) addresses.SetDefault(UserId, saved.Id);
            Notice("Adres kaydedildi", $"{saved.Title} adresin adres defterinde.", "success");
        }
        catch (StoreValidationException exception) { Notice("Adres kaydedilemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult MakeDefault(int id)
    {
        try { addresses.SetDefault(UserId, id); }
        catch (StoreValidationException exception) { Notice("Adres güncellenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try { addresses.Delete(UserId, id); Notice("Adres silindi", "Adres defterinden kaldırıldı.", "success"); }
        catch (StoreValidationException exception) { Notice("Adres silinemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index));
    }

    private void Notice(string title, string message, string css)
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
}
