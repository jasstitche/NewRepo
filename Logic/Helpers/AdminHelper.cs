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
                    AppUser = cart.AppUser,
                    UserId = userId,
                    Quantity = change,
                    SubTotal = samplePage.Price,
                    SamplesPage = samplePage,
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
                if (paymentId != 0)
                {
                    var regApprove = _context.Payments.Where(x => x.Id == paymentId && x.PaymentVerificationStatus == PaymentVerificationStatus.Sent).Include(x => x.ApplicationsUser).Include(x => x.Orders).FirstOrDefault();
                    if (regApprove != null)
                    {

                        regApprove.PaymentVerificationStatus = PaymentVerificationStatus.Seen;
                        regApprove.ApprovedBy = loggedInUser;
                        regApprove.ApprovedDate = DateTime.Now;
                        _context.Update(regApprove);



                        var updateUserOrder = _context.Orders.Where(x => x.UserId == regApprove.ApplicationsUser.Id && x.Payment.Id == paymentId).FirstOrDefault();
                        if (updateUserOrder != null)
                        {
                            updateUserOrder.OrderStatus = OrderStatus.Completed;
                            updateUserOrder.ApproveDate = DateTime.Now;
                            _context.Update(updateUserOrder);
                            _context.SaveChanges();

                            if (regApprove.ApplicationsUser?.Email != null)
                            {
                                string toEmail = regApprove.ApplicationsUser?.Email;
                                string subject = "Hooray!!!, Payment Approved ";
                                string message = "Hello " + "<b>" + regApprove.ApplicationsUser?.UserName + ", </b>" + "<br>		Jas_p Stitches	 has approved your payment of ₦" + " <b> " + regApprove.Orders?.TotalAmount.ToString() + ". </b> <br> " +

                               " we	at Jas_p Stitches	Appreciate your patronage." +
                               " <br> we look forward for more patronage. <br> " +
                               " Thank you !!! ";
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
                    var regDecline = _context.Payments
                                             .Where(x => x.Id == paymentId && x.PaymentVerificationStatus == PaymentVerificationStatus.Sent)
                                             .Include(x => x.ApplicationsUser)
                                             .Include(x => x.Orders)
                                             .FirstOrDefault();
                    if (regDecline != null)
                    {
                        regDecline.PaymentVerificationStatus = PaymentVerificationStatus.Declined;
                        regDecline.ApprovedBy = loggedInUser;
                        regDecline.ApprovedDate = DateTime.Now;
                        _context.Update(regDecline);

                        _context.SaveChanges();

                        if (!string.IsNullOrEmpty(regDecline.ApplicationsUser?.Email))
                        {
                            string toEmail = regDecline.ApplicationsUser.Email;
                            string subject = "Payment Declined";
                            string message = $"Hello <b>{regDecline.ApplicationsUser.UserName},</b><br>" +
                                             $"We regret to inform you that your payment of ₦<b>{regDecline.Orders.TotalAmount}</b> has been declined.<br>" +
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
    }

}
