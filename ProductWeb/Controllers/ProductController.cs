using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductWeb.ViewModels;

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
        public IActionResult UpCreate(int? id)
        {
            var productVM = new ProductVM()
            {
                Product = new(),
                CategoryList = productContext.Categories.Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                })
            };
            if (id != null && id != 0)
            {
                /*Update*/
                productVM.Product = productContext.Products.Find(id);
                if (productVM.Product == null)
                {
                    return RedirectToAction(nameof(Index));
                }

            }
            return View(productVM);
        }
        [HttpPost]
        public IActionResult UpCreate(ProductVM productVM)
        {
            var id = productVM.Product.Id;
            if (id != 0)
            {
                /*Update*/
                productContext.Update(productVM.Product);
            }
            else
            {
                /*Create*/
                productContext.Add(productVM.Product);
            }
            productContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var product = productContext.Products.Find(id);
            if (product==null)
            {
                /*Update*/
                TempData["Message"] = "Not Found.";
                return RedirectToAction(nameof(Index));
            }
            productContext.Remove(product);
            productContext.SaveChanges();
            TempData["Message"] = "Delete Successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
