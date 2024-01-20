using Microsoft.AspNetCore.Mvc;

namespace ProductWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductContext productContext;

        public ProductController(ProductContext productContext)
        {
            this.productContext = productContext;
        }
        public IActionResult Index()
        {
            var products = productContext.Products.Include(p => p.Category).ToList();
            return View(products);
        }
    }
}
