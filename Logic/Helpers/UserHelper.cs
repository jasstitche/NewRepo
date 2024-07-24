using Core.Data;
using Core.Enum;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Enum.eFashionEnum;
using static System.Net.Mime.MediaTypeNames;

namespace Logic.Helpers
{
    public class UserHelper : IUserHelper
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public UserHelper(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context; 
        }

        public async Task<ApplicationUser> CreateUserByAsync(ApplicationUserViewModel applicationUserViewModel)
        {
            if (applicationUserViewModel != null)
            {
                var user = new ApplicationUser()
                {
                    FirstName = applicationUserViewModel.FirstName,
                    LastName = applicationUserViewModel.LastName,
                    DateCreated = DateTime.Now,
                    PhoneNumber = applicationUserViewModel.PhoneNumber,
                    UserName = applicationUserViewModel.Email,
                    Email = applicationUserViewModel.Email,
                    Gender = applicationUserViewModel.Gender,
                    Role = applicationUserViewModel.Role,
                    Active  = applicationUserViewModel.Active,
                    IsDeactivated = applicationUserViewModel.IsDeactivated,
                };
                var result = await _userManager.CreateAsync(user, applicationUserViewModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, applicationUserViewModel.Role);
                    return user;
                }
            }
            return null;
        }
        public ApplicationUser FindUserByEmail(string email)
        {
            var user = _userManager.Users.Where(x => x.Email == email).FirstOrDefault();
            return user;
        }

        public List<DropdownEnumModel> GetDropDownEnumsList()        {            return ((PaymentType[])Enum.GetValues(typeof(PaymentType))).Select(c => new DropdownEnumModel() { Id = (int)c, Name = c.ToString() }).ToList();        }

        //public async Task<bool> SavePaymentDetails(OrdersViewModel ordersViewModel,string userId, string unqueFileName)
        //{
        //    if (ordersViewModel.UploadPayment != null)
        //    {
        //        var paymentDetails = new Payment()
        //        {
        //            PaidBy = userId,
        //           PaymentType = PaymentType.Transfer,
        //           PaymentVerificationStatus = PaymentVerificationStatus.Sent,
        //           PaymentReceipt = unqueFileName,
        //           PaymentDate = DateTime.Now,
        //        };
        //         _context.Payments.Add(paymentDetails);

        //        var cartItems = _context.Carts.Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active).Include(c => c.SamplesPage).Include(c => c.AppUser).ToList();

        //        foreach (var cartItem in cartItems)
        //        {
        //            var order = new Orders()
        //            {
        //                FirstName = cartItem.AppUser.FirstName,
        //                LastName = cartItem.AppUser.LastName,
        //                PhoneNumber = cartItem.AppUser.PhoneNumber,
        //                EmailAddress = cartItem.AppUser.Email,
        //                UserId = userId,
        //                SampleId = cartItem.SamplesPage.id ?? 0,
        //                SubTotal = cartItem.SubTotal,
        //                OrderStatus = OrderStatus.Shipped,
        //                PaymentType = PaymentType.Transfer,
        //                OrderDate = DateTime.Now,
        //                TotalAmount = cartItems.Sum(c => c.SubTotal),
        //                // DeliveryStartDate = ordersViewModel.DeliveryStartDate,
        //                // DeliveryEndDate = ordersViewModel.DeliveryEndDate,
        //                // PickUpAddress = ordersViewModel.PickUpAddress,
        //                // CustomersAddress = ordersViewModel.CustomersAddress
        //            };
        //            _context.Orders.Add(order);
        //        }
        //        _context.SaveChanges();
        //    }
        //    return true;
        //}

        public async Task<bool> SavePaymentDetails(OrdersViewModel ordersViewModel, string userId, string uniqueFileName)
        {
            if (ordersViewModel.UploadPayment != null)
            {
                var paymentDetails = new Payment()
                {
                    PaidBy = userId,
                    PaymentType = ordersViewModel.PaymentTypeId,
                    PaymentVerificationStatus = PaymentVerificationStatus.Sent,
                    PaymentReceipt = uniqueFileName,
                    PaymentDate = DateTime.Now,
                    ApprovedBy = userId,
                    TotalAmount = ordersViewModel.TotalAmount,
                };
                _context.Payments.Add(paymentDetails);
                await _context.SaveChangesAsync();

                var cartItems = _context.Carts
                                        .Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active)
                                        .Include(c => c.SamplesPage)
                                        .Include(c => c.AppUser)
                                        .ToList();

                foreach (var cartItem in cartItems)
                {
                    var order = new Orders()
                    {
                        FirstName = cartItem.AppUser.FirstName,
                        LastName = cartItem.AppUser.LastName,
                        PhoneNumber = cartItem.AppUser.PhoneNumber,
                        EmailAddress = cartItem.AppUser.Email,
                        UserId = userId,
                        SampleId = cartItem.SamplesPage.id ?? 0,
                        SubTotal = new List<decimal?> { cartItem.SubTotal },
                        OrderStatus = OrderStatus.Shipped,
                        PaymentType = paymentDetails.PaymentType,
                        OrderDate = DateTime.Now,
                        TotalAmount = paymentDetails.TotalAmount,
                        PaymentId = paymentDetails.Id,
                    };
                    _context.Orders.Add(order);
                }

                var savedOrder = _context.Orders.FirstOrDefault(o => o.UserId == userId && o.OrderDate == DateTime.Now);
                if (savedOrder != null)
                {
                    paymentDetails.OrdersId = savedOrder.Id;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            return false;
        }

        //public async Task<bool> SavePaymentDetails(OrdersViewModel ordersViewModel, string userId, string uniqueFileName)
        //{
        //    if (ordersViewModel.UploadPayment != null)
        //    {
        //        var paymentDetails = new Payment()
        //        { 
        //            PaidBy = userId,
        //            PaymentType = ordersViewModel.PaymentTypeId,
        //            PaymentVerificationStatus = PaymentVerificationStatus.Sent,
        //            PaymentReceipt = uniqueFileName,
        //            PaymentDate = DateTime.Now,
        //            ApprovedBy = userId,
        //            //OrdersId = ordersViewModel.Payment.,
        //            //TotalAmount = ordersViewModel.TotalAmountToPay
        //        };
        //        _context.Payments.Add(paymentDetails);
        //        await _context.SaveChangesAsync();


        //        var cartItems = _context.Carts
        //                                .Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active)
        //                                .Include(c => c.SamplesPage)
        //                                .Include(c => c.AppUser)
        //                                .ToList();

        //        foreach (var cartItem in cartItems)
        //        {
        //            var order = new Orders()
        //            {
        //                FirstName = cartItem.AppUser.FirstName,
        //                LastName = cartItem.AppUser.LastName,
        //                PhoneNumber = cartItem.AppUser.PhoneNumber,
        //                EmailAddress = cartItem.AppUser.Email,
        //                UserId = userId,
        //                SampleId = cartItem.SamplesPage.id ?? 0,
        //                SubTotal = new List<decimal?> { cartItem.SubTotal },
        //                OrderStatus = OrderStatus.Shipped,
        //                PaymentType = paymentDetails.PaymentType,
        //                OrderDate = DateTime.Now,
        //                TotalAmount = ordersViewModel.TotalAmountToPay,
        //                //PaymentVerificationStatus = paymentDetails.PaymentVerificationStatus,
        //            };
        //            _context.Orders.Add(order);
        //        }

        //        var orderId = _context.Orders.FirstOrDefault()?.Id;
        //        var paymentOrder = new Payment()
        //        {

        //            OrdersId = orderId.Value,
        //        };
        //        await _context.SaveChangesAsync();

        //        return true;
        //    }
        //    return false;
        //}

        public bool CheckIfPendingPayment(string userId)
        {
            if (userId != null)
            {
                var verify = _context.Payments.Where(c => c.PaidBy == userId).Any();
              if (verify)
                {
                    return true;
                }
            }
            return false;
        }


        public bool CheckIfApproved(int paymentId)
        {
            if (paymentId != 0)
            {
                return _context.Payments.Where(x => x.Id == paymentId && x.PaymentVerificationStatus == PaymentVerificationStatus.Seen).Include(x => x.ApplicationsUser).Any();
            }
            return false;
        }
        public bool CheckUserRegPayment(string userId)
        {
            if (userId != null)
            {
                var orderPay = _context.Payments.Where(s => s.ApplicationsUser.Id == userId && s.PaymentVerificationStatus == PaymentVerificationStatus.Sent && s.PaymentVerificationStatus == PaymentVerificationStatus.Seen).FirstOrDefault();
                if (orderPay != null)
                {
                    return true;
                }
            }
            return false;
        }
        public class DropDown
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
        }
        public class DropdownEnumModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
