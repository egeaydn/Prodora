using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers;

public class HomeController : Controller
{
    // Unhandled exceptions and bad URLs end up here. The request ID lets a customer quote it and lets us find the log line.
    public IActionResult Error(int? code)
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        ViewData["NotFound"] = code == 404;
        Response.StatusCode = code is >= 400 and < 600 ? code.Value : 500;
        return View();
    }
   private IProductServices _productServices;

	public HomeController(IProductServices productServices)
	{
		_productServices = productServices;
	}
	public IActionResult Index()
    {
       var products = _productServices.GetAll();
		if (products == null || !products.Any())
		{
			products = new List<Product>();
		}
		return View(new ProductListModel()
		{
			Products = products
		});
	}
   
}
