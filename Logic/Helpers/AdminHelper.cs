using Core.Config;
using Core.Data;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static Core.Enum.eFashionEnum;

namespace Logic.Helpers
{
    public class AdminHelper : IAdminHelper
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private IGeneralConfiguration _generalConfiguration;


        public AdminHelper(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IGeneralConfiguration generalConfiguration)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _generalConfiguration = generalConfiguration;
        }

        public async Task<ApplicationUser> FindByUserNameAsync(string username)
        {
            return await _userManager.Users.Where(s => s.UserName == username)?.FirstOrDefaultAsync();
        }



        public string GetCurrentUserId(string username)
        {
            return _userManager.Users.Where(s => s.UserName == username)?.FirstOrDefaultAsync().Result.Id?.ToString();
        }



        public string GetUserId(string username)
        {
            return _userManager.Users.Where(s => s.UserName == username)?.FirstOrDefaultAsync().Result.Id?.ToString();
        }

        public ApplicationUser FindByUserName(string username)
        {
            return _userManager.Users.Where(s => s.UserName == username)?.FirstOrDefault();
        }

        public SamplePage CreateSample(SamplePage samplePage, string unqueFileName)
        {

            if (samplePage != null)
            {
                var sample = new SamplePage()
                {
                    DesignName = samplePage.DesignName,
                    MaterialName = samplePage.MaterialName,
                    DateSampled = DateTime.Now,
                    ClothSize = samplePage.ClothSize,
                    Price = samplePage.Price,
                    File = unqueFileName,
                    NumberOfItem = samplePage.NumberOfItem,
                };
                _context.Add(sample);
                _context.SaveChanges();


                return sample;

            }
            return null;
        }

        public Cart CreateCart(int sampleId, string userId)
        {
            var sample = _context.SamplePages.Where(s => s.id == sampleId).FirstOrDefault();
            if (userId != null && sampleId > 0)
            {
                var cart = new Cart()
                {

                    Quantity = 1,
                    Active = true,
                    Deleted = false,
                    UserId = userId,
                    SampleId = sampleId,
                    SubTotal = sample.Price
                };
                _context.Add(cart);
                _context.SaveChanges();


                return cart;

            }
            return null;

        }


        //public bool CreatCart(int sampleId)
        // {
        //     if (sampleId > 0) 
        //     {
        //         var cart = new Cart()
        //         {
        //             SampleId = sampleId,
        //         };
        //         _context.Add(cart);
        //         _context.SaveChanges();

        //         return true;

        //     }
        //     return false; 

        // }

        public async Task<List<Cart>> GetAllCart(string email)
        {
            var cartViewModel = new List<Cart>();
            var loggedInUser = _userManager.Users.Where(x => x.UserName == email).FirstOrDefault();
            var userCart = await _context.Carts.Where(x => x.UserId == loggedInUser.Id && x.Deleted == false && x.Active == true).Include(u => u.SamplesPage).Include(Carts => Carts.AppUser).ToListAsync();
            var cart = userCart.Select(ct => new Cart
            {
                Id = ct.Id,
                Quantity = ct.Quantity,
                SubTotal = ct.SubTotal,
                SamplesPage = ct.SamplesPage,


            });
            if (userCart.Any())
            {
                return userCart;
            }
            return cartViewModel;
        }
        public async Task<Cart> DeleteCartbyId(int id)
        {
            var cart = _context.Carts.Where(x => x.Id == id).Include(x => x.AppUser).FirstOrDefault();
            if (cart != null)
            {
                cart.Deleted = true;
                cart.Active = false;
                _context.Update(cart);
                _context.SaveChanges();
            }
            return cart;
        }

        public async Task<Cart> UpdateQuantityProcess(int id, int change, string userId)
        {
            var cart = _context.Carts.Where(x => x.Id == id).Include(x => x.AppUser).Include(x => x.SamplesPage).FirstOrDefault();
            if (cart != null && cart.Quantity != null)
            {
                var addedQuantity = cart.Quantity += change;
                if (cart.Quantity < 1)
                {
                    cart.Quantity = 1;
                }
                cart.SubTotal = addedQuantity * cart.SamplesPage.Price;
                _context.Update(cart);

            }
            else
            {
                var samplePage = await _context.SamplePages.FindAsync(id);
                cart = new Cart
                {
                    SampleId = samplePage.id,
                    //AppUser = cart.AppUser,
                    UserId = userId,
                    Quantity = change,
                    SubTotal = samplePage.Price,
                    //SamplesPage = samplePage,
                };
                _context.Carts.Add(cart);
            }
            _context.SaveChanges();
            return cart;
        }

        public bool ApproveOrderPayment(int paymentId, string loggedInUser)
        {
            try
            {
                var companySettings = GetCompanySetting();
                var existingSettings = _context.CompanySetting.FirstOrDefault();

                if (paymentId != 0)
                {

                        var paymentApprove = _context.Payments.Where(x => x.Id == paymentId && x.PaymentVerificationStatus == PaymentVerificationStatus.Sent).Include(x => x.ApplicationsUser).Include(x => x.Orders).FirstOrDefault();
                        if (paymentApprove != null && paymentApprove.PaymentType == PaymentType.Transfer)
                        {

                            paymentApprove.PaymentVerificationStatus = PaymentVerificationStatus.Seen;
                            paymentApprove.OrderStatus = OrderStatus.Shipped;
                            paymentApprove.ApprovedBy = loggedInUser;
                            paymentApprove.ApprovedDate = DateTime.Now;
                            _context.Update(paymentApprove);
                            _context.SaveChanges();


                            DateTime? pickUpDate = null;

                            if (paymentApprove.ApprovedDate.HasValue)
                            {
                                // Assuming you want to add a fixed number of 5 days
                                int daysToAdd = 5;
                                pickUpDate = paymentApprove.ApprovedDate.Value.AddDays(daysToAdd);
                            }


                            var updateUserOrder = _context.Orders.Where(x => x.UserId == paymentApprove.PaidBy && x.Payment.Id == paymentId).FirstOrDefault();
                            if (updateUserOrder != null)
                            {
                                updateUserOrder.OrderStatus = OrderStatus.Shipped;
                                updateUserOrder.ApproveDate = paymentApprove.ApprovedDate;
                                updateUserOrder.PaymentVerificationStatus = paymentApprove.PaymentVerificationStatus;
                                updateUserOrder.DeliveryStartDate = pickUpDate;
                                _context.Update(updateUserOrder);
                                _context.SaveChanges();


                                var sampleIds = _context.Carts
                                    .Where(c => c.UserId == paymentApprove.PaidBy && !c.Deleted)
                                    .Select(c => c.SampleId)
                                    .Distinct()
                                    .ToList();

                                // Update cart items in bulk
                                var cartsToUpdate = _context.Carts
                                    .Where(c => c.UserId == paymentApprove.PaidBy && sampleIds.Contains(c.SampleId))
                                    .ToList();

                                if (cartsToUpdate.Any())
                                {
                                    foreach (var cart in cartsToUpdate)
                                    {
                                        cart.Active = false;
                                        cart.Deleted = true;
                                    }
                                    _context.Carts.UpdateRange(cartsToUpdate);
                                    _context.SaveChanges();
                                }


                                if (paymentApprove.Orders?.EmailAddress != null)
                                {

                                    string toEmail = paymentApprove.Orders?.EmailAddress;
                                    string subject = "Hooray!!!, Payment Approved ";
                                    string message = $"Hello <b>{updateUserOrder.FirstName},</b><br> " +
                                                     $"Jas_p Stitches has approved your payment of ₦<b>{paymentApprove.Orders?.TotalAmount.ToString()}</b>.<br> " +
                                                     $"We at Jas_p Stitches appreciate your patronage.<br> " +
                                                     $"Your order will be ready for pickup on <b>{pickUpDate:dddd, MMMM dd, yyyy}</b>.<br> " +
                                                     "We look forward to your continued patronage.<br> " +
                                                     "Thank you!!!";

                                    _emailService.SendEmail(toEmail, subject, message);
                                    return true;

                                }
                            }

                        }
                    else if (paymentId != null && paymentApprove.PaymentType == PaymentType.Cash)
                    {
                        paymentApprove.PaymentVerificationStatus = PaymentVerificationStatus.Completed;
                        paymentApprove.OrderStatus = OrderStatus.Completed;
                        paymentApprove.ApprovedBy = loggedInUser;
                        paymentApprove.ApprovedDate = DateTime.Now;
                        _context.Update(paymentApprove);
                        _context.SaveChanges();


                        DateTime? pickUpDate = null;

                        if (paymentApprove.ApprovedDate.HasValue)
                        {
                            // Assuming you want to add a fixed number of 5 days
                            int daysToAdd = 5;
                            pickUpDate = paymentApprove.ApprovedDate.Value.AddDays(daysToAdd);
                        }


                        var updateUserOrder = _context.Orders.Where(x => x.UserId == paymentApprove.PaidBy && x.Payment.Id == paymentId).FirstOrDefault();
                        if (updateUserOrder != null)
                        {
                            updateUserOrder.OrderStatus = paymentApprove.OrderStatus;
                            updateUserOrder.ApproveDate = paymentApprove.ApprovedDate;
                            updateUserOrder.PaymentVerificationStatus = paymentApprove.PaymentVerificationStatus;
                            updateUserOrder.DeliveryStartDate = pickUpDate;
                            _context.Update(updateUserOrder);
                            _context.SaveChanges();


                            var sampleIds = _context.Carts
                                .Where(c => c.UserId == paymentApprove.PaidBy && !c.Deleted)
                                .Select(c => c.SampleId)
                                .Distinct()
                                .ToList();

                            // Update cart items in bulk
                            var cartsToUpdate = _context.Carts
                                .Where(c => c.UserId == paymentApprove.PaidBy && sampleIds.Contains(c.SampleId))
                                .ToList();

                            if (cartsToUpdate.Any())
                            {
                                foreach (var cart in cartsToUpdate)
                                {
                                    cart.Active = false;
                                    cart.Deleted = true;
                                }
                                _context.Carts.UpdateRange(cartsToUpdate);
                                _context.SaveChanges();
                            }


                            if (paymentApprove.Orders?.EmailAddress != null)
                            {

                                string toEmail = paymentApprove.Orders?.EmailAddress;
                                string subject = "Hooray!!!, Payment Approved ";
                                string message = $"Hello <b>{updateUserOrder.FirstName},</b><br> " +
                                                 $"Jas_p Stitches has approved your payment of ₦<b>{paymentApprove.Orders?.TotalAmount.ToString()}</b>.<br> " +
                                                 $"We at Jas_p Stitches appreciate your patronage.<br> " +
                                                 $"Your order will be ready for pickup on <b>{pickUpDate:dddd, MMMM dd, yyyy}</b>.<br> " +
                                                 "We look forward to your continued patronage.<br> " +
                                                 "Thank you!!!";

                                _emailService.SendEmail(toEmail, subject, message);
                                return true;

                            }
                        }

                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool DeclineOrderPayment(int paymentId, string loggedInUser)
        {
            try
            {
                if (paymentId != 0)
                {
                    var paymentDecline = _context.Payments
                                             .Where(x => x.Id == paymentId && x.PaymentVerificationStatus == PaymentVerificationStatus.Sent)
                                             .Include(x => x.ApplicationsUser)
                                             .Include(x => x.Orders)
                                             .FirstOrDefault();
                    if (paymentDecline != null)
                    {
                        paymentDecline.OrderStatus = OrderStatus.Cancelled;
                        paymentDecline.PaymentVerificationStatus = PaymentVerificationStatus.Declined;
                        paymentDecline.ApprovedBy = loggedInUser;
                        paymentDecline.ApprovedDate = DateTime.Now;
                        _context.Update(paymentDecline);
                        _context.SaveChanges();

                        var updateUserOrder = _context.Orders.Where(x => x.UserId == paymentDecline.PaidBy && x.Payment.Id == paymentId).FirstOrDefault();
                        if (updateUserOrder != null)
                        {
                            updateUserOrder.OrderStatus = OrderStatus.Cancelled;
                            updateUserOrder.ApproveDate = paymentDecline.ApprovedDate;
                            updateUserOrder.PaymentVerificationStatus = paymentDecline.PaymentVerificationStatus;
                            _context.Update(updateUserOrder);
                            _context.SaveChanges();
                        }

                        var sampleIds = _context.Orders
                        .Where(os => os.Id == updateUserOrder.Id)
                        .Select(os => os.SampleId)
                        .ToList();

                        var carts = _context.Carts
                            .Where(c => c.UserId == paymentDecline.PaidBy && sampleIds.Contains(c.SampleId) && !c.Deleted)
                            .ToList();

                        foreach (var cart in carts)
                        {
                            cart.Active = false;
                            cart.Deleted = true;
                            _context.Update(cart);
                            _context.SaveChanges();
                        }
                        if (!string.IsNullOrEmpty(paymentDecline.ApplicationsUser?.Email))
                        {
                            string toEmail = paymentDecline.ApplicationsUser.Email;
                            string subject = "Payment Declined";
                            string message = $"Hello <b>{paymentDecline.ApplicationsUser.UserName},</b><br>" +
                                             $"We regret to inform you that your payment of ₦<b>{paymentDecline.Orders.TotalAmount}</b> has been declined.<br>" +
                                             "Please contact our support team for more information.<br>" +
                                             "Thank you.";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                throw;
            }
        }

        public bool CheckIfDeclined(int paymentId)
        {
            var payment = _context.Payments
                                  .Where(x => x.Id == paymentId)
                                  .FirstOrDefault();
            return payment != null && payment.PaymentVerificationStatus == PaymentVerificationStatus.Declined;
        }

        //public CompanySetting CreateCompanySettings(CompanySettingViewModel companySetting)
        //{

        //    if (companySetting != null)
        //    {
        //        var setting = new CompanySetting()
        //        {
        //            CompanyAddress = companySetting.CompanyAddress,
        //            AccountNumber = companySetting.AccountNumber,
        //            AccountName = companySetting.AccountName,
        //            BankName = companySetting.BankName,
        //            DeliveryAddress = companySetting.DeliveryAddress,
        //            DeliveryFee = companySetting.DeliveryFee,
        //            PickUpDays = companySetting.PickUpDays,
        //        };
        //        _context.CompanySetting.Add(companySetting);
        //        _context.SaveChanges();


        //        return setting;

        //    }
        //    return null;
        //}
        public async Task<CompanySettingViewModel> GetCompanySetting()
        {
            var settings = new CompanySettingViewModel();

            var existingSetting = _context.CompanySetting.FirstOrDefault();

            if (existingSetting == null)
            {
                var newCompanySetting = new CompanySetting()
                {
                    CompanyAddress = _generalConfiguration.CompanyAddress,
                    AccountNumber =  _generalConfiguration.AccountNumber,
                    AccountName = _generalConfiguration.AccountName ,
                    BankName = _generalConfiguration.BankName ,
                    DeliveryAddress = _generalConfiguration.DeliveryAddress ,
                    DeliveryFee = _generalConfiguration.DeliveryFee ,
                    PickUpDays = _generalConfiguration.PickUpDays,
                    
                };

                _context.Add(newCompanySetting);
                _context.SaveChanges();

                settings.CompanyAddress = newCompanySetting.CompanyAddress;
                settings.AccountNumber = newCompanySetting.AccountNumber;
                settings.AccountName = newCompanySetting.AccountName;
                settings.BankName = newCompanySetting.BankName;
                settings.DeliveryAddress = newCompanySetting.DeliveryAddress;
                settings.DeliveryFee = newCompanySetting.DeliveryFee;
                settings.PickUpDays = newCompanySetting.PickUpDays;
                return settings;

            }
            settings.CompanyAddress = existingSetting.CompanyAddress;
            settings.AccountNumber = existingSetting.AccountNumber;
            settings.AccountName = existingSetting.AccountName;
            settings.BankName = existingSetting.BankName;
            settings.DeliveryAddress = existingSetting.DeliveryAddress;
            settings.DeliveryFee = existingSetting.DeliveryFee;
            settings.PickUpDays = existingSetting.PickUpDays;
            return settings;
        }

        public CompanySettingViewModel UpdateCompanySettings(CompanySettingViewModel companySettingViewModel)
        {
            try
            {
                var existingSettings = _context.CompanySetting.FirstOrDefault();

                if (existingSettings != null)
                {
                    existingSettings.CompanyAddress = companySettingViewModel.CompanyAddress;
                    existingSettings.AccountNumber = companySettingViewModel.AccountNumber;
                    existingSettings.AccountName = companySettingViewModel.AccountName;
                    existingSettings.BankName = companySettingViewModel.BankName;
                    existingSettings.DeliveryFee= companySettingViewModel.DeliveryFee;
                    existingSettings.PickUpDays = companySettingViewModel.PickUpDays;
                    existingSettings.DeliveryAddress = companySettingViewModel.DeliveryAddress;
                    _context.CompanySetting.Update(existingSettings);
                    _context.SaveChanges();

                    var updatedViewModel = new CompanySettingViewModel
                    {
                        CompanyAddress = existingSettings.CompanyAddress,
                        AccountNumber = existingSettings.AccountNumber,
                        AccountName = existingSettings.AccountName,
                        BankName = existingSettings.BankName,
                        DeliveryFee = existingSettings.DeliveryFee,
                        PickUpDays = existingSettings.PickUpDays,
                        DeliveryAddress = existingSettings.DeliveryAddress
                    };

                    return updatedViewModel;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> CreateAdminByAsync(ApplicationUserViewModel applicationUserViewModel)
        {
            if (applicationUserViewModel != null)
            {
                var admin = new ApplicationUser()
                {
                    FirstName = applicationUserViewModel.FirstName,
                    LastName = applicationUserViewModel.LastName,
                    DateCreated = DateTime.Now,
                    PhoneNumber = applicationUserViewModel.PhoneNumber,
                    UserName = applicationUserViewModel.Email,
                    Email = applicationUserViewModel.Email,
                    Gender = applicationUserViewModel.Gender,
                    //Role = applicationUserViewModel.Role,
                    //Active  = applicationUserViewModel.Active,
                    Active = true,
                    //IsDeactivated = applicationUserViewModel.IsDeactivated,
                    IsDeactivated = false,
                };
                var result = await _userManager.CreateAsync(admin, applicationUserViewModel.Password);
                if (result.Succeeded)
                {
                    var userRoles = await _userManager.GetRolesAsync(admin);
                    if (!userRoles.Contains("Admin"))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(admin, "Admin");
                        if (!roleResult.Succeeded)
                        {
                            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            throw new Exception($"Role assignment failed: {roleErrors}");
                        }
                    }
                    //await _userManager.AddToRoleAsync(user, applicationUserViewModel.Role);
                    //return user;
                }
                return admin;

            }
            return null;
        }
    }

}
