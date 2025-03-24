using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers;

public class HomeController : Controller
{
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
