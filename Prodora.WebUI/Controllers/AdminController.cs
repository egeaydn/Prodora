using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prodora.Business.Abstract;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Controllers
{
	public class AdminController : Controller
	{
		private IProductServices _productServices;	
		private ICategoryServices _categoryServices;
		private UserManager<IdentityUser> _userManager;
		private RoleManager<IdentityRole> _roleManager;

		public AdminController(IProductServices productServices, ICategoryServices categoryServices,UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_categoryServices = categoryServices;
			_productServices = productServices;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		[Route("admin/products")]
		public IActionResult ProductList()
		{
			return View(
				new ProductListModel()
				{
					Products = _productServices.GetAll()
				}
			);
		}
	}
}
