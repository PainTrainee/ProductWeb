using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProductWeb.Controllers
{

    [Authorize]
    public class CartController : Controller
    {
        private readonly ProductContext productContext;
        private readonly ShoppingCartService shoppingCartService;
        private readonly IWebHostEnvironment webHostEnvironment;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(ProductContext productContext, ShoppingCartService shoppingCartService,
            IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.shoppingCartService = shoppingCartService;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new()
            {
                ListCart = productContext.ShoppingCarts.Include(p => p.Product).Where(p => p.UserId == userId).ToList(),
                OrderHeader = new()
            };
            if (ShoppingCartVM.ListCart.Count() == 0)
            {
                TempData["Message"] = "คุณยังไม่ได้เลือกสินค้า";
                return RedirectToAction("Index", "Home");
            }
            foreach (var item in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += item.Product.Price * item.Count;
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Plus(int cartId)
        {
            var cart = productContext.ShoppingCarts.Find(cartId);
            shoppingCartService.IncrementCount(cart, 1);
            shoppingCartService.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = productContext.ShoppingCarts.Find(cartId);
            if (cart.Count > 1)
            {
                shoppingCartService.DecrementCount(cart, 1);
                shoppingCartService.Save();
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cart = productContext.ShoppingCarts.Find(cartId);
            productContext.Remove(cart);
            productContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = productContext.ShoppingCarts.Include(x => x.Product).Where(u => u.UserId.Equals(userId)),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.User = productContext.Users.Find(userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.User.FullName;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.User.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.User.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.User.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.User.PostalCode;
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        public IActionResult SummaryPost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ShoppingCartVM.
                ListCart = productContext.ShoppingCarts.Include(x => x.Product).Where(u => u.UserId.Equals(userId));
            ShoppingCartVM.OrderHeader.UserId = userId;
            ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            //foreach (var cart in ShoppingCartVM.ListCart)
            //{
            //    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price *
            //    cart.Count);
            //}
            //var user = productContext.Users.Find(claim.Value);
            #region Image Management
            string wwwRootPath = webHostEnvironment.WebRootPath;
            var file = ShoppingCartVM.file;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(file.FileName);
                var uploads = wwwRootPath + SD.PaymentPath;
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);
                //บันทึกรุปภาพใหม่
                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                ShoppingCartVM.OrderHeader.PaymentImage = fileName + extension;
            }
            #endregion
            productContext.OrderHeaders.Add(ShoppingCartVM.OrderHeader); //One
            productContext.SaveChanges();
            foreach (var cart in ShoppingCartVM.ListCart) //Many
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Count = cart.Count

                };
                productContext.OrderDetails.Add(orderDetail);
                // productContext.SaveChanges();
            }
            productContext.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCart);//ลบตะกร้า
            productContext.SaveChanges();
            TempData["Message"] = "successful transaction";
            return RedirectToAction("Index", "Home");
        }
    }
}
