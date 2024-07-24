using Core.Data;
using Core.Models;
using Core.ViewModels;
using Logic.Helpers;
using Logic.IHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace eFashion.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IAdminHelper _adminHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;



        public OrdersController(ApplicationDbContext context, IUserHelper userHelper, IWebHostEnvironment webHostEnvironment, IAdminHelper adminHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _webHostEnvironment = webHostEnvironment;
            _adminHelper = adminHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orderItemsDetails = await _context.Orders
                                                  .Where(c => c.UserId == userId)
                                                  .Include(c => c.SamplePage)
                                                  .Include(c => c.ApplicationUser)
                                                  .Include(c => c.Payment)
                                                  .Select(order => new OrdersViewModel
                                                  {
                                                      FirstName = order.FirstName,
                                                      LastName = order.LastName,
                                                      UserId = order.UserId,
                                                      SampleId = order.SampleId,
                                                      SubTotal = order.SubTotal, // Handling List<decimal?>
                                                      OrderStatus = order.OrderStatus,
                                                      PaymentId = order.Payment.Id,
                                                      OrderDate = order.OrderDate,
                                                      TotalAmount = order.TotalAmount ?? 0, // Handle nullable
                                                      Payment = order.Payment,
                                                      PaymentVerificationStatus = order.Payment.PaymentVerificationStatus,
                                                      //TotalQuantity = order
                                                  })
                                                  .ToListAsync(); // Await the asynchronous call

            return View(orderItemsDetails);
        }



        [HttpGet]
        public IActionResult Orderspage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _context.Carts
                                    .Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active)
                                    .Include(c => c.SamplesPage)
                                    .Include(c => c.AppUser)
                                    .ToList();

            ViewBag.PaymentType = _userHelper.GetDropDownEnumsList();

            if (cartItems == null || !cartItems.Any())
            {
                return View("Error", new { message = "Cart is empty" });
            }
            var getCompanySettings = _adminHelper.GetCompanySetting().Result;
            var deliveryFee = getCompanySettings?.DeliveryFee != null ? Convert.ToDecimal(getCompanySettings.DeliveryFee) : 0;


            var model = new OrdersViewModel()
            {
                TotalAmount = cartItems.Sum(c => c.SubTotal),
                DesignName = cartItems.Select(c => c.SamplesPage.DesignName).ToList(),
                Quantity = cartItems.Select(c => c.Quantity).ToList(),
                TotalQuantity = cartItems.Sum(c => c.Quantity),
                SubTotal = cartItems.Select(c => c.SubTotal).ToList(),
                TotalAmountToPay = cartItems.Sum(c => c.SubTotal) + deliveryFee,
          
                //CompanySettings = getCompanySettings
            };

            return View(model);
        }

        public async Task<IActionResult> ConfirmPayment(OrdersViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            string unqueFileName = String.Empty;
            if (viewModel.UploadPayment != null)
            {
                unqueFileName = UploadedFile(viewModel.UploadPayment);
            }
            var checkIfPaid = _userHelper.CheckIfPendingPayment(userId);
            if (checkIfPaid)
            {
                return View("Has made payment");
            }
            var savePaymentDetials = await _userHelper.SavePaymentDetails( viewModel, userId,  unqueFileName);

            if (savePaymentDetials)
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Handle failure case
                return View("Error", new { message = "Failed to save payment details." });
            }
            //return RedirectToAction("Index");
        }

        public string UploadedFile(IFormFile filesSender)
        {
            string uniqueFileName = string.Empty;

            if (filesSender != null && filesSender.Length > 0)
            {
                // Define the path to the folder where files will be uploaded
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "paymentupload");

                // Create the folder if it does not exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name using a GUID
                uniqueFileName = Guid.NewGuid().ToString() + "_" + filesSender.FileName;

                // Combine the folder path and the unique file name to get the full path
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    filesSender.CopyTo(fileStream);
                }

                // Return the relative path to the file (this will be used in the view)
                return Path.Combine("paymentupload", uniqueFileName).Replace("\\", "/");
            }

            return null;
        }

        //[HttpGet]
        //public JsonResult Orderspage()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var cartItems = _context.Carts.Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active).Include(c => c.SamplesPage).Include(c => c.AppUser).ToList();
        //    ViewBag.PaymentType = _userHelper.GetDropDownEnumsList();
        //    if (cartItems == null || !cartItems.Any())
        //    {
        //       return Json(new { isError = true, message = "Cart is empty" });
        //    }
        //    var model = new OrdersViewModel()
        //    {
        //        TotalAmount = cartItems.Sum(c => c.SubTotal),
        //        DesignName = cartItems.Select(c => c.SamplesPage.DesignName).ToList(),
        //        Quantity = cartItems.Select(c => c.Quantity).ToList(),
        //        TotalQuantity = cartItems.Sum(c => c.Quantity),
        //    };

        //    return Json(new { isError = false,
        //        totalAmount = model.TotalAmount,
        //        designName = model.DesignName,
        //        quantity = model.Quantity,
        //        totalQuantity = model.TotalQuantity
        //    });
        //}
    }
}
