using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Audit;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers;

[Authorize(Roles = "admin")]
public class AdminVariantsController(IVariantServices variants, IProductServices products, IAuditLog audit) : Controller
{
    [HttpGet]
    public IActionResult Index(int productId)
    {
        var product = products.GetProductDetail(productId);
        if (product == null) return NotFound();
        ViewData["Product"] = product;
        return View(variants.GetByProduct(productId));
    }

    [HttpPost]
    public IActionResult Add(int productId, string? name, int quantity)
    {
        try
        {
            var added = variants.Add(productId, name ?? "", quantity);
            audit.Record("variant.add", $"Ürün #{productId} · {added.Name}", $"stok {added.StockQuantity}");
            Notice("Seçenek eklendi", $"{added.Name} ürüne eklendi.", "success");
        }
        catch (StoreValidationException exception) { Notice("Seçenek eklenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost]
    public IActionResult SetStock(int variantId, int quantity)
    {
        var variant = variants.Get(variantId);
        if (variant == null) return NotFound();
        try
        {
            variants.SetStock(variantId, quantity);
            audit.Record("variant.stock", $"Ürün #{variant.ProductId} · {variant.Name}", $"stok {variant.StockQuantity} → {quantity}");
        }
        catch (StoreValidationException exception) { Notice("Stok güncellenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index), new { productId = variant.ProductId });
    }

    [HttpPost]
    public IActionResult SetActive(int variantId, bool isActive)
    {
        var variant = variants.Get(variantId);
        if (variant == null) return NotFound();
        try
        {
            variants.SetActive(variantId, isActive);
            audit.Record("variant.toggle", $"Ürün #{variant.ProductId} · {variant.Name}", isActive ? "Açıldı" : "Kapatıldı");
        }
        catch (StoreValidationException exception) { Notice("Seçenek güncellenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index), new { productId = variant.ProductId });
    }

    private void Notice(string title, string message, string css)
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
}
