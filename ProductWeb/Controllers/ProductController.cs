using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductWeb.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductContext productContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(ProductContext productContext,IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var products = productContext.Products.Include(p => p.Category).ToList();
            foreach (var item in products)
            {
                if (item.ImageUrl != null)
                    item.ImageUrl = SD.ProductPath + "\\" + item.ImageUrl;
            }
            return View(products.OrderByDescending(p=>p.Id));
        }
        public IActionResult UpCreate(int? id)
        {
            var productVM = new ProductVM()
            {
                Product = new()
                {
                    Name = "TestProduct",
                    Price = 1,
                    Description = "Test Description"
                },
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
            if(!ModelState.IsValid) { return View(productVM); }
            #region Image Management
            string wwwRootPath = webHostEnvironment.WebRootPath;
            var file = productVM.File;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(file.FileName);

                var uploads = wwwRootPath + SD.ProductPath; //wwwroot\images\product
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);
                //กรณีมีรูปภาพเดิมตอ้งลบทิ้งก่อน
                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(uploads, productVM.Product.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                //บันทึกรุปภาพใหม่

                using (var fileStreams = new FileStream(Path.Combine(uploads,fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }

                productVM.Product.ImageUrl = fileName + extension;
            }
            #endregion Image Management
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
            if (product.ImageUrl != null)
            {
                var oldImagePath = webHostEnvironment.WebRootPath + SD.ProductPath + "\\" + product.ImageUrl;
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            //wwwroot\images\products\Name.jpg
            productContext.Remove(product);
            productContext.SaveChanges();
            TempData["Message"] = "Delete Successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
