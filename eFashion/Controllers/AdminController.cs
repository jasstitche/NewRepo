using Core.Data;
using Core.Models;
using Core.ViewModels;
using Logic.Helpers;
using Logic.IHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using static Core.Enum.eFashionEnum;

namespace eFashion.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IAdminHelper _adminHelper;
        private readonly IDropdownHelper _dropdownHelper;


        public AdminController(ApplicationDbContext context, IAdminHelper adminHelper, IUserHelper userHelper, IDropdownHelper dropdownHelper)
        {
            _context = context;
            _adminHelper = adminHelper;
            _userHelper = userHelper;
            _dropdownHelper = dropdownHelper;
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {

            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> PaymentApproval()
        //{
        //    var allPaymentsQuery = await _context.Payments
        //                                         .Include(x => x.Applicationuser)
        //                                         .Include(x => x.Orders)
        //                                         .OrderByDescending(s => s.PaymentDate)
        //                                         .ToListAsync();

        //    var model = new List<PaymentViewModels>();

        //    if (allPaymentsQuery.Any())
        //    {
        //        foreach (var payment in allPaymentsQuery)
        //        {
        //            var paymentViewModel = new PaymentViewModels()
        //            {
        //                Id = payment.Id,
        //                PaidBy = payment.ApplicationsUser != null ? payment.ApplicationsUser.FirstName + " " + payment.ApplicationsUser.LastName : "Unknown",
        //                ApprovedBy = payment.ApplicationsUser?.UserName,
        //                PaymentType = payment.PaymentType,
        //                PaymentDate = payment.PaymentDate,
        //                ApprovedDate = payment.ApprovedDate,
        //                PaymentReceipt = payment.PaymentReceipt,
        //                OrdersId = payment.Orders != null ? payment.Orders.Id : (int?)null ,// assuming Orders.Id is an int
        //                PaymentVerificationStatus = payment.PaymentVerificationStatus,
        //                OrderStatus = payment.OrderStatus,
        //            };
        //            model.Add(paymentViewModel);
        //        }
        //    }

        //    return View(model);
        //}

        [HttpGet]
        public async Task<IActionResult> PaymentApproval()
        {
            var allPaymentsQuery = await _context.Payments
                                                 .Include(x => x.Applicationuser)
                                                 .Include(x => x.ApplicationsUser)
                                                 .Include(x => x.Orders)
                                                 .OrderByDescending(s => s.PaymentDate)
                                                 .ToListAsync();

            var model = new List<PaymentViewModels>();

            if (allPaymentsQuery.Any())
            {
                var allUsers = _context.Users.ToList(); // Load users into memory (or query in a more optimized way)

                foreach (var payment in allPaymentsQuery)
                {
                    var approvedByUser = _context.Users.OfType<ApplicationUser>()
                        .FirstOrDefault(u => u.Id == payment.ApprovedBy);
                    var paymentViewModel = new PaymentViewModels()
                    {
                        Id = payment.Id,
                        PaidBy = payment.Applicationuser != null ? payment.Applicationuser.FirstName + " " + payment.Applicationuser.LastName : "Unknown",
                        //ApprovedBy = payment.ApplicationsUser?.UserName,
                        PaymentType = payment.PaymentType,
                        PaymentDate = payment.PaymentDate,
                        ApprovedDate = payment.ApprovedDate,
                        PaymentReceipt = payment.PaymentReceipt,
                        OrdersId = payment.Orders != null ? payment.Orders.Id : (int?)null,
                        PaymentVerificationStatus = payment.PaymentVerificationStatus,
                        OrderStatus = payment.Orders.OrderStatus,
                        ApprovedBy = approvedByUser != null ? approvedByUser.FirstName + " " + approvedByUser.LastName : "Unknown", // Concatenate first and last names
                    };
                    model.Add(paymentViewModel);
                }
            }

            return View(model);
        }

        public async Task<JsonResult> ApprovePayment(int paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != 0)
                {
                    var user = _adminHelper.GetCurrentUserId(User.Identity.Name);
                    var payment = _context.Payments.Where(x => x.Id == paymentId).Include(x => x.ApplicationsUser).FirstOrDefault();
                    var checkifApprovedBefore = _userHelper.CheckIfApproved(paymentId);
                    if (checkifApprovedBefore)
                    {
                        return Json(new { isError = true, msg = "This payment has  been approved before" });
                    }
                    //var checkIfUserHasPaid = _userHelper.CheckUserRegPayment(payment.ApplicationsUser.Id);
                    //if (checkIfUserHasPaid)
                    //{
                    //    return Json(new { isError = true, msg = "This user has already paid" });
                    //}
                    var approve = _adminHelper.ApproveOrderPayment(paymentId, user);
                    if (approve)
                    {
                        return Json(new { isError = false, msg = "Order Payment has been approved" });
                    }
                }
                return Json(new { isError = true, msg = "Could not find Payment to approve" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        public async Task<JsonResult> DeclinePayment(int paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != 0)
                {
                    var user = _adminHelper.GetCurrentUserId(User.Identity.Name);
                    var payment = await _context.Payments
                                                .Where(x => x.Id == paymentId)
                                                .Include(x => x.ApplicationsUser)
                                                .FirstOrDefaultAsync();

                    if (payment == null)
                    {
                        return Json(new { isError = true, msg = "Could not find Payment to decline" });
                    }

                    var checkIfDeclinedBefore = _adminHelper.CheckIfDeclined(paymentId);
                    if (checkIfDeclinedBefore)
                    {
                        return Json(new { isError = true, msg = "This payment has been declined before" });
                    }

                    var decline = _adminHelper.DeclineOrderPayment(paymentId, user);
                    if (decline)
                    {
                        return Json(new { isError = false, msg = "Order Payment has been declined" });
                    }
                }
                return Json(new { isError = true, msg = "Could not find Payment to decline" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> CompanySettings()
        {

            ViewBag.States = await _dropdownHelper.GetState();
            var CompanySettings = _adminHelper.GetCompanySetting().Result;
                if (CompanySettings != null)
                {
                    return View(CompanySettings);
                }
            

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanySettings()
        {
            ViewBag.States = await _dropdownHelper.GetState();

            var companySettings = await _adminHelper.GetCompanySetting();
            if (companySettings != null)
            {
                return Json(new { isError = false, data = companySettings });
            }
            return Json(new { isError = true, msg = "No data found" });
        }

        [HttpPost]
        public async Task<IActionResult> CompanySettings(CompanySettingViewModel companySettingViewModel)
        {
            if (companySettingViewModel != null)
            {

                var saveCompanySettings = _adminHelper.UpdateCompanySettings(companySettingViewModel);
                    if (saveCompanySettings != null)
                    {
                    return View(saveCompanySettings);

                    //return RedirectToAction("AdminDashBoard", "Admin");
                    }
                
            }
            return View(companySettingViewModel);


        }

        public async Task<JsonResult> Received(int paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != 0)
                {
                    var user = _adminHelper.GetCurrentUserId(User.Identity.Name);
                    var checkIfReceivedOrder = _userHelper.CheckIfReceived(paymentId, user);
                    if (checkIfReceivedOrder)
                    {
                        return Json(new { isError = true, msg = "This user has received order before" });

                    }
                    var upDateOrder = _userHelper.UpdateOrderToCompleted(paymentId, user);
                    return Json(new { isError = false, msg = "Order received!" });
                }
                return Json(new { isError = true, msg = "Could not find order" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllStates()
        {
            var states = _adminHelper.GetAllStates();
            return View(states);
        }


        [HttpPost]
        public async Task<IActionResult> States(StateViewModel stateViewModel)
        {
            if (stateViewModel != null)
            {

                var savedStates = _adminHelper.UpdateStates(stateViewModel);
                if (savedStates != null)
                {
                    return RedirectToAction("CompanySettings", "Admin");

                    //return RedirectToAction("AdminDashBoard", "Admin");
                }

            }
            return View(stateViewModel);


        }

        [HttpGet]
        public JsonResult GetDeliveryFee(int stateId)
        {
            // Assuming you have a method in your service or repository to get the delivery fee by stateId
            var deliveryFee = _adminHelper.GetDeliveryFeeByStateId(stateId);
            return Json(deliveryFee);
        }

        [HttpGet]
        public JsonResult GetStateDetails(int stateId)
        {
            var state = _adminHelper.GetDeliveryFeeByStateId(stateId); // Assume this method fetches the state details
            if (state != null)
            {
                return Json(new
                {
                    id = state.Id,
                    name = state.Name,
                    deliveryFee = state.DeliveryFee,
                    active = state.Active,
                    deleted = state.Deleted
                });
            }
            return Json(null);
        }

    }

}
