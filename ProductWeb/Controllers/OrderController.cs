using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductWeb.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ProductContext productContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(ProductContext productContext, IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var orderHeaders = productContext.OrderHeaders.ToList();
            return View(orderHeaders);
        }
        public IActionResult Details(int id)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = productContext.OrderHeaders.Include(x => x.User).FirstOrDefault(x => x.Id == id),
                OrderDetail = productContext.OrderDetails.Include(x => x.Product).Where(x => x.Id == id)
            };
            orderVM.OrderHeader.PaymentImage = SD.PaymentPath + "\\" + orderVM.OrderHeader.PaymentImage;
            return View(orderVM);
        }
        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderHeader()
        {
            var orderHeaderFromDb = productContext.OrderHeaders.Find(OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            productContext.OrderHeaders.Update(orderHeaderFromDb);
            productContext.SaveChanges();
            TempData["Success"] = "Order Header Updated Successfully.";
            return RedirectToAction(nameof(Details), "Order", new { id = orderHeaderFromDb.Id });
        }
        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StatusOrder(string status)
        {
            var orderHeaderFromDb = productContext.OrderHeaders.Find(OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb.OrderStatus == SD.StatusPending)
            {
                orderHeaderFromDb.OrderStatus = status;
                TempData["Message"] = "Status has been updated Succesfully.";
                productContext.SaveChanges();
            }
            else
            {
                TempData["Message"] = "Can't update because status has ended.";
            }
            return RedirectToAction("Details", "Order", new { id = OrderVM.OrderHeader.Id });
        }
    }
}
