﻿using Core.Data;
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

        public AdminController(ApplicationDbContext context, IAdminHelper adminHelper, IUserHelper userHelper)
        {
            _context = context;
            _adminHelper = adminHelper;
            _userHelper = userHelper;
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentApproval()
        {
            var allPaymentsQuery = await _context.Payments
                                                 .Include(x => x.ApplicationsUser)
                                                 .Include(x => x.Orders)
                                                 .OrderByDescending(s => s.PaymentDate)
                                                 .ToListAsync();

            var model = new List<PaymentViewModels>();

            if (allPaymentsQuery.Any())
            {
                foreach (var payment in allPaymentsQuery)
                {
                    var paymentViewModel = new PaymentViewModels()
                    {
                        Id = payment.Id,
                        PaidBy = payment.ApplicationsUser != null ? payment.ApplicationsUser.FirstName + " " + payment.ApplicationsUser.LastName : "Unknown",
                        ApprovedBy = payment.ApplicationsUser?.UserName,
                        PaymentType = payment.PaymentType,
                        PaymentDate = payment.PaymentDate,
                        ApprovedDate = payment.ApprovedDate,
                        PaymentReceipt = payment.PaymentReceipt,
                        OrdersId = payment.Orders != null ? payment.Orders.Id : (int?)null ,// assuming Orders.Id is an int
                        PaymentVerificationStatus = payment.PaymentVerificationStatus,
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
                    var checkIfUserHasPaid = _userHelper.CheckUserRegPayment(payment.ApplicationsUser.Id);
                    if (checkIfUserHasPaid)
                    {
                        return Json(new { isError = true, msg = "This user has already paid" });
                    }
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
        public IActionResult CompanySettings()
        {

 
                var CompanySettings = _adminHelper.GetCompanySetting().Result;
                if (CompanySettings != null)
                {
                    return View(CompanySettings);
                }
            

            return View();
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
                    }
                
            }
            return View();


        }
       

    }

}