using Microsoft.AspNetCore.Mvc;

namespace ProductWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ProductContext productContext;

        public CategoryController(ProductContext productContext)
        {
            this.productContext = productContext;
        }
        public IActionResult Index()
        {
            var result = this.productContext.Categories.ToList();
            return View(result);
        }
        public IActionResult UpCreate(int? id)
        {
            var category = new Category();
            if (id == 0 || id == null) { /*Create*/ }
            else
            {
                /*Update*/
                category = productContext.Categories.Find(id);
                if (category == null)
                {
                    TempData["Message"] = "Not Found.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult UpCreate(Category category)
        {
            var id = category.Id;
            if (id == 0) 
            { 
                /*Create*/
                productContext.Categories.Add(category);
            }
            else
            {
                /*Update*/
                productContext.Update(category);
            }
            productContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Delete(int id)
        {
            var product = productContext.Categories.Find(id);
            productContext.Categories.Remove(product);
            productContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}