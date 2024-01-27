using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ProductWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductContext productContext;
        private readonly ShoppingCartService shoppingCartService;

        public HomeController(ProductContext productContext, ShoppingCartService shoppingCartService)
        {
            this.productContext = productContext;
            this.shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            var products = productContext.Products.ToList();
            foreach (var item in products)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    item.ImageUrl = SD.ProductPath + "\\" + item.ImageUrl;
                }
            }
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            var product = productContext.Products.Include(p => p.Category)
                .FirstOrDefault(x => x.Id.Equals(productId));
            if (product == null)
            {
                TempData["Message"] = "Not Found.";
                return RedirectToAction(nameof(Index));
            }

            ShoppingCart shoppingCart = new()
            {
                Product = product,
                Count = 1,
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize] // ตรวจสอบสิทธิ์ตาม role
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity; // Id
            //var user = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //shoppingCart.UserId = user.Value;
            shoppingCart.UserId = userId;
            //productContext.Add(shoppingCart);

            var cartFromDb = productContext.ShoppingCarts.FirstOrDefault(x => x.UserId == shoppingCart.UserId && x.ProductId == shoppingCart.ProductId);
            if (cartFromDb == null) 
            {
                //ยังไม่เคยหยิบใส่ตะกร้า
                shoppingCartService.Add(shoppingCart);
            }
            else
            {
                //มีในตะกร้า
                shoppingCartService.IncrementCount(cartFromDb,shoppingCart.Count);
            }
            shoppingCartService.Save();
            return RedirectToAction(nameof(Index));
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
