using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Extensions;
using Prodora.WebUI.Audit;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers;

[Authorize(Roles = "admin")]
public class AdminCouponsController(ICouponServices coupons, IAuditLog audit) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(coupons.GetAll());

    [HttpPost]
    public IActionResult Create(string code, CouponKind kind, decimal value, decimal minSubtotal, DateTime? expiresAt, int? maxRedemptions)
    {
        try
        {
            // The expiry date is inclusive: the coupon works until the end of that day.
            coupons.Create(new Coupon
            {
                Code = code, Kind = kind, Value = value, MinSubtotal = minSubtotal,
                ExpiresAt = expiresAt?.Date.AddDays(1), MaxRedemptions = maxRedemptions
            });
            audit.Record("coupon.create", $"Kupon {code.Trim().ToUpperInvariant()}", $"{(kind == CouponKind.Percent ? $"%{value:0}" : $"{value:0.00} TL")}, en az sepet {minSubtotal:0.00} TL, son gün {(expiresAt?.ToString("dd.MM.yyyy") ?? "yok")}, sınır {(maxRedemptions?.ToString() ?? "yok")}");
            Notice("Kupon oluşturuldu", $"{code.Trim().ToUpperInvariant()} kuponu kullanıma hazır.", "success");
        }
        catch (StoreValidationException exception) { Notice("Kupon oluşturulamadı", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SetActive(int id, bool isActive)
    {
        try { coupons.SetActive(id, isActive); audit.Record("coupon.toggle", $"Kupon #{id}", isActive ? "Açıldı" : "Kapatıldı"); }
        catch (StoreValidationException exception) { Notice("Kupon güncellenemedi", exception.Message, "warning"); }
        return RedirectToAction(nameof(Index));
    }

    private void Notice(string title, string message, string css)
        => TempData.Put("message", new ResultModels { Title = title, Message = message, Css = css });
}
