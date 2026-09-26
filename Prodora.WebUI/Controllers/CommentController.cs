using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;
namespace Prodora.WebUI.Controllers;

public class CommentController(ICommentServices comments, IProductServices products, UserManager<ApplicationUser> users,
    ILogger<CommentController> logger) : Controller
{
    [HttpGet] public Task<IActionResult> ShowProductComments(int? id) => GetComments(id);
    [HttpGet]
    public async Task<IActionResult> GetComments(int? productId)
    {
        if (productId is null or <= 0) return BadRequest("Ürün seçilmedi.");
        var product = products.GetById(productId.Value);
        if (product == null || product.IsArchived) return NotFound();
        ViewBag.ProductId = productId.Value;
        return await Render(comments.GetCommentByProductId(productId.Value));
    }
    [HttpPost, Authorize]
    public IActionResult Create(CommentModel model)
    {
        if (!ModelState.IsValid || model.ProductId is null or <= 0 || string.IsNullOrWhiteSpace(model.Text))
            return Json(new { result = false, message = "Geçerli bir ürün, yorum ve 1–5 arasında puan gir." });
        var userId = users.GetUserId(User);
        if (userId == null) return Challenge();
        var product = products.GetById(model.ProductId.Value);
        if (product == null || product.IsArchived) return Json(new { result = false, message = "Ürün bulunamadı." });
        try
        {
            comments.Create(new Comment { CommentText = model.Text.Trim(), ProductId = product.Id, UserId = userId,
                Raitings = model.Raiting, CreatedAt = DateTime.Now });
            return Json(new { result = true });
        }
        catch (Exception exception)
        {
            logger.LogError("Comment creation failed; error type {ErrorType}", exception.GetType().Name);
            return StatusCode(500, new { result = false, message = "Yorum kaydedilemedi. Biraz sonra tekrar dene." });
        }
    }
    [HttpPost, Authorize]
    public IActionResult Delete(int? id)
    {
        if (id is null or <= 0) return BadRequest("Yorum seçilmedi.");
        var comment = comments.GetById(id.Value);
        if (comment == null) return NotFound();
        if (comment.UserId != users.GetUserId(User)) return Forbid();
        comments.Delete(comment);
        return Json(new { result = true });
    }
    [HttpGet]
    public async Task<IActionResult> GetCommentsByUser(string? userId)
        => string.IsNullOrWhiteSpace(userId) ? BadRequest("Kullanıcı seçilmedi.") : await Render(comments.GetCommentsByUserId(userId));
    [HttpGet]
    public async Task<IActionResult> GetCommentsByUserName(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return BadRequest("Kullanıcı adı gerekli.");
        var user = await users.FindByNameAsync(userName);
        return await Render(user == null ? new() : comments.GetCommentsByUserId(user.Id));
    }
    [HttpGet]
    public async Task<IActionResult> GetCommentsByProductName(string? productName)
        => string.IsNullOrWhiteSpace(productName) ? BadRequest("Ürün adı gerekli.") : await Render(comments.GetCommentsByProductName(productName));
    [HttpGet]
    public async Task<IActionResult> GetCommentsByBrand(string? brandName)
        => string.IsNullOrWhiteSpace(brandName) ? BadRequest("Marka gerekli.") : await Render(comments.GetCommentsByProductBrand(brandName));
    [HttpGet]
    public async Task<IActionResult> GetCommentsByPriceRange(decimal min, decimal max)
        => min < 0 || max < min ? BadRequest("Fiyat aralığı geçersiz.") : await Render(comments.GetCommentsByProductPriceRange(min, max));
    [HttpGet]
    public async Task<IActionResult> GetCommentsByRating(int rating)
        => rating < 1 || rating > 5 ? BadRequest("Puan 1 ile 5 arasında olmalı.") : await Render(comments.GetCommentsByProductRating(rating));
    [HttpGet]
    public async Task<IActionResult> GetCommentsByDateRange(DateTime startDate, DateTime endDate)
        => startDate > endDate ? BadRequest("Tarih aralığı geçersiz.") : await Render(comments.GetCommentsByDateRange(startDate, endDate));
    [HttpGet]
    public IActionResult GetAverageRating(int productId)
    {
        var product = products.GetById(productId);
        return product == null || product.IsArchived ? NotFound() : Json(new { rating = comments.GetAverageRating(productId) });
    }
    private async Task<IActionResult> Render(List<Comment> items)
    {
        var names = new Dictionary<string, string>();
        foreach (var id in items.Select(c => c.UserId).Distinct())
            names[id] = (await users.FindByIdAsync(id))?.FullName ?? "Prodora kullanıcısı";
        ViewBag.Usernames = names;
        return PartialView("_PartialComments", items);
    }
}
