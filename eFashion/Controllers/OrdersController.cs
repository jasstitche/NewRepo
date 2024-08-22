using Core.Data;
using Core.Models;
using Core.ViewModels;
using eFashion.Models;
using Logic.Helpers;
using Logic.IHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using static Core.Enum.eFashionEnum;

namespace eFashion.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IAdminHelper _adminHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDropdownHelper _dropdownHelper;




        public OrdersController(ApplicationDbContext context, IUserHelper userHelper, IWebHostEnvironment webHostEnvironment, IAdminHelper adminHelper, IDropdownHelper dropdownHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _webHostEnvironment = webHostEnvironment;
            _adminHelper = adminHelper;
            _dropdownHelper = dropdownHelper;
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
                                                  .OrderByDescending(c => c.OrderDate)
                                                  
                                                  .Select(order => new OrdersViewModel
                                                  {
                                                      FirstName = order.FirstName,
                                                      LastName = order.LastName,
                                                      UserId = order.UserId,
                                                      SampleId = order.SampleId,
                                                      SubTotal = new List<decimal?> { order.SubTotal }, // Handling List<decimal?>
                                                      OrderStatus = order.OrderStatus,
                                                      PaymentId = order.Payment.Id,
                                                      OrderDate = order.OrderDate,
                                                      TotalAmount = order.TotalAmount ?? 0, // Handle nullable
                                                      Payment = order.Payment,
                                                      PaymentVerificationStatus = order.Payment.PaymentVerificationStatus,
                                                      PaymentTypeId = order.PaymentType,
                                                      DeliveryStartDate = order.DeliveryStartDate,
                                                      //TotalQuantity = order
                                                  })
                                                  .ToListAsync(); // Await the asynchronous call

            return View(orderItemsDetails);
        }

        [HttpGet]
        public IActionResult GetDeliveryFee(int stateId)
        {
            var deliveryFee = _context.States.Where(s => s.Id == stateId).FirstOrDefault()?.DeliveryFee;
            return Json(new { DeliveryFee = deliveryFee });
        }

        //[HttpGet]
        //public IActionResult GetDeliveryFees(int stateId)
        //{

        //    var deliveryFee = _context.States
        //                              .Where(s => s.Id == stateId)
        //                              .Select(s => s.DeliveryFee)
        //                              .FirstOrDefault();

        //    if (deliveryFee == null)
        //    {
        //        return Json(new { deliveryFee = "N/A" }); // Handle cases where no fee is found
        //    }

        //    return Json(new { deliveryFee = deliveryFee });
        //}


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
            //ViewBag.States =  _dropdownHelper.GetState();

            ViewBag.States = new SelectList(_context.States, "Id", "Name");

            if (cartItems == null || !cartItems.Any())
            {
                //return View("Error", new { message = "Cart is empty" });

                var errorModel = new ErrorViewModel
                {
                    Message = "Cart is empty",
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                };
                return View("Error", errorModel);

            }



            var model = new OrdersViewModel()
            {
                TotalAmount = cartItems.Sum(c => c.SubTotal),
                DesignName = cartItems.Select(c => c.SamplesPage.DesignName).ToList(),
                Quantity = cartItems.Select(c => c.Quantity).ToList(),
                TotalQuantity = cartItems.Sum(c => c.Quantity),
                SubTotal = cartItems.Select(c => c.SubTotal).ToList(),
                TotalAmountToPay = cartItems.Sum(c => c.SubTotal),
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
            //var checkIfPaid = _userHelper.CheckIfPendingPayment(userId);
            //if (checkIfPaid)
            //{
            //    TempData["ErrorMessage"] = "You already have a pending order.";
            //    return RedirectToAction("Orderspage");
            //}
            var savePaymentDetials = await _userHelper.SavePaymentDetails( viewModel, userId,  unqueFileName);

            if (savePaymentDetials)
            {
                TempData["SuccessMessage"] = "Order details saved successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to save order details. Please try again.";
                return View(viewModel);
            }
            //return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmCashPayment(OrdersViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //string unqueFileName = String.Empty;
            //if (viewModel.UploadPayment != null)
            //{
            //    unqueFileName = UploadedFile(viewModel.UploadPayment);
            //}
            //var checkIfPaid = _userHelper.CheckIfPendingPayment(userId);
            //if (checkIfPaid)
            //{
            //    return RedirectToAction("Has made payment");
            //}
            if (viewModel.PaymentTypeId == PaymentType.Cash)
            {
                // Subtract DeliveryFee from TotalAmountToPay
                viewModel.TotalAmountToPay = (viewModel.TotalAmountToPay ?? 0) - (viewModel.DeliveryFee ?? 0);
            }
            var savePaymentDetials = await _userHelper.SaveCashPaymentDetails(viewModel, userId);

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
        [HttpPost]
        public async Task<JsonResult> Shipped(int paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != 0)
                {
                    var userId = _adminHelper.GetCurrentUserId(User.Identity.Name);
                    if (userId == null)
                    {
                        return Json(new { isError = true, msg = "User not found" });
                    }

                    var checkIfReceivedOrder = _userHelper.CheckIfReceived(paymentId, userId);
                    if (checkIfReceivedOrder)
                    {
                        return Json(new { isError = true, msg = "This user has received the order before" });
                    }

                    var updateOrder = _userHelper.UpdateOrder(paymentId, userId);
                    if (updateOrder)
                    {
                        return Json(new { isError = false, msg = "Order received!" });
                    }
                    else
                    {
                        return Json(new { isError = true, msg = "Order update failed" });
                    }
                }
                return Json(new { isError = true, msg = "Invalid payment ID" });
            }
            catch (Exception ex)
            {

                return Json(new { isError = true, msg = "An error occurred: " + ex.Message });
            }
        }

    }
}
