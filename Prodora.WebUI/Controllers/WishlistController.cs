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
public class WishlistController(IWishlistServices wishlist, UserManager<ApplicationUser> users) : Controller
{
    private string UserId => users.GetUserId(User) ?? throw new InvalidOperationException("Oturum bulunamadı.");

    [HttpGet]
    public IActionResult Index() => View(wishlist.GetProducts(UserId));

    [HttpPost]
    public IActionResult Toggle(int productId, string? returnUrl)
    {
        try { wishlist.Toggle(UserId, productId); }
        catch (StoreValidationException exception)
        {
            TempData.Put("message", new ResultModels { Title = "Favoriler", Message = exception.Message, Css = "warning" });
        }
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }
}
