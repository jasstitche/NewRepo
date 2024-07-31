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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAdminHelper _adminHelper;


        public OrderStatus OrderStatus { get; private set; }

        public UserHelper(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context, RoleManager<IdentityRole> roleManager,IAdminHelper adminHelper)
        {
            _userManager = userManager;
            _context = context; 
            _roleManager = roleManager;
            _adminHelper = adminHelper;
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
                    Address = applicationUserViewModel.Address,
                   // Role = applicationUserViewModel.Role,
                    //Active  = applicationUserViewModel.Active,
                    Active  = true,
                    //IsDeactivated = applicationUserViewModel.IsDeactivated,
                    IsDeactivated = false,
                };
                var result = await _userManager.CreateAsync(user, applicationUserViewModel.Password);
                if (result.Succeeded)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (!userRoles.Contains("User"))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, "User");
                        if (!roleResult.Succeeded)
                        {
                            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            throw new Exception($"Role assignment failed: {roleErrors}");
                        }
                    }
                    //await _userManager.AddToRoleAsync(user, applicationUserViewModel.Role);
                    //return user;
                }
                return user;

            }
            return null;
        }
        public ApplicationUser FindUserByEmail(string email)
        {
            var user = _userManager.Users.Where(x => x.Email == email).FirstOrDefault();
            return user;
        }

        public List<DropdownEnumModel> GetDropDownEnumsList()        {            return ((PaymentType[])Enum.GetValues(typeof(PaymentType))).Select(c => new DropdownEnumModel() { Id = (int)c, Name = c.ToString() }).ToList();        }

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
        //            //ApprovedBy = userId,
        //            TotalAmount = ordersViewModel.TotalAmountToPay,
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
        //                SubTotal =  cartItem.SubTotal,
        //                OrderStatus = OrderStatus.Awaiting,
        //                PaymentType = paymentDetails.PaymentType,
        //                OrderDate = DateTime.Now,
        //                TotalAmount = paymentDetails.TotalAmount,
        //                PaymentId = paymentDetails.Id,
        //                TotalQuantity = cartItem.Quantity,
        //                PaymentVerificationStatus = paymentDetails.PaymentVerificationStatus,
        //                PaymentDate = paymentDetails.PaymentDate,
        //                CustomersAddress = cartItem.AppUser.Address,
        //            };
        //            _context.Orders.Add(order);
        //            await _context.SaveChangesAsync();
        //        }

        //        paymentDetails.OrdersId = order.Id;
        //        _context.Payments.Update(paymentDetails);
        //        await _context.SaveChangesAsync();

        //        return true;
        //    }
        //    return false;
        //}
        public async Task<bool> SavePaymentDetails(OrdersViewModel ordersViewModel, string userId, string uniqueFileName)
        {
            if (ordersViewModel.UploadPayment != null)
            {
                var getCompanySettings = _adminHelper.GetCompanySetting().Result;
                var deliveryFee = getCompanySettings?.DeliveryFee != null ? Convert.ToDecimal(getCompanySettings.DeliveryFee) : 0;
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create the payment record
                    var paymentDetails = new Payment()
                    {
                        PaidBy = userId,
                        PaymentType = ordersViewModel.PaymentTypeId,
                        PaymentVerificationStatus = PaymentVerificationStatus.Sent,
                        PaymentReceipt = uniqueFileName,
                        PaymentDate = DateTime.Now,
                        TotalAmount = ordersViewModel.TotalAmountToPay + deliveryFee,
                        OrderStatus = OrderStatus.Awaiting,
                    };
                    _context.Payments.Add(paymentDetails);
                    await _context.SaveChangesAsync();

                    // Create the order records
                    var cartItems = _context.Carts
                                            .Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active)
                                            .Include(c => c.SamplesPage)
                                            .Include(c => c.AppUser)
                                            .ToList();

                    var firstOrderId = 0; // Variable to hold the first order ID

                    if (cartItems.Any())
                    {
                        var firstCartItem = cartItems.First();

                        var order = new Orders()
                        {
                            FirstName = firstCartItem.AppUser.FirstName,
                            LastName = firstCartItem.AppUser.LastName,
                            PhoneNumber = firstCartItem.AppUser.PhoneNumber,
                            EmailAddress = firstCartItem.AppUser.Email,
                            UserId = userId,
                            SampleId = firstCartItem.SamplesPage.id ?? 0,
                            SubTotal = cartItems.Sum(c => c.SubTotal),
                            OrderStatus = OrderStatus.Awaiting,
                            PaymentType = paymentDetails.PaymentType,
                            OrderDate = DateTime.Now,
                            TotalAmount = paymentDetails.TotalAmount,
                            TotalQuantity = cartItems.Sum(c => c.Quantity),
                            PaymentVerificationStatus = paymentDetails.PaymentVerificationStatus,
                            PaymentDate = paymentDetails.PaymentDate,
                            CustomersAddress = firstCartItem.AppUser.Address,
                            PaymentId = paymentDetails.Id, // Assign payment ID here
                        };
                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        // Save the first order ID
                        if (firstOrderId == 0)
                        {
                            firstOrderId = order.Id;
                        }
                        //}
                    }
                        // Update the payment record with the first order ID
                        paymentDetails.OrdersId = firstOrderId;
                        _context.Payments.Update(paymentDetails);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            return false;
        }

        public async Task<bool> SaveCashPaymentDetails(OrdersViewModel ordersViewModel, string userId)
        {
            //if (ordersViewModel.UploadPayment != null)
            //{
                using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create the payment record
                var paymentDetails = new Payment()
                {
                    PaidBy = userId,
                    PaymentType = ordersViewModel.PaymentTypeId,
                    PaymentVerificationStatus = PaymentVerificationStatus.Sent,
                    PaymentDate = DateTime.Now,
                    TotalAmount = ordersViewModel.TotalAmountToPay,
                    OrderStatus = OrderStatus.Awaiting,
                };
                _context.Payments.Add(paymentDetails);
                await _context.SaveChangesAsync();

                // Create the order records
                var cartItems = _context.Carts
                                        .Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active)
                                        .Include(c => c.SamplesPage)
                                        .Include(c => c.AppUser)
                                        .ToList();

                var firstOrderId = 0; // Variable to hold the first order ID

                if (cartItems.Any())
                {
                    var firstCartItem = cartItems.First();

                    var order = new Orders()
                    {
                        FirstName = firstCartItem.AppUser.FirstName,
                        LastName = firstCartItem.AppUser.LastName,
                        PhoneNumber = firstCartItem.AppUser.PhoneNumber,
                        EmailAddress = firstCartItem.AppUser.Email,
                        UserId = userId,
                        SampleId = firstCartItem.SamplesPage.id ?? 0,
                        SubTotal = cartItems.Sum(c => c.SubTotal),
                        OrderStatus = OrderStatus.Awaiting,
                        PaymentType = paymentDetails.PaymentType,
                        OrderDate = DateTime.Now,
                        TotalAmount = paymentDetails.TotalAmount,
                        TotalQuantity = cartItems.Sum(c => c.Quantity),
                        PaymentVerificationStatus = paymentDetails.PaymentVerificationStatus,
                        PaymentDate = paymentDetails.PaymentDate,
                        CustomersAddress = firstCartItem.AppUser.Address,
                        PaymentId = paymentDetails.Id, // Assign payment ID here
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Save the first order ID
                    if (firstOrderId == 0)
                    {
                        firstOrderId = order.Id;
                    }
                    //}
                }
                // Update the payment record with the first order ID
                paymentDetails.OrdersId = firstOrderId;
                _context.Payments.Update(paymentDetails);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            //}
            //return false;
        }
        public bool CheckIfPendingPayment(string userId)
        {
            if (userId != null)
            {
                var verify = _context.Payments.Where(c => c.PaidBy == userId && c.PaymentVerificationStatus != PaymentVerificationStatus.Completed && c.OrderStatus != OrderStatus.Completed ).Any();
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

        public bool CheckIfReceived(int paymentId, string userId)
        {
            if (userId != null)
            {
                var orderStatus = _context.Orders.Where(s => s.UserId == userId && s.PaymentId == paymentId && s.OrderStatus == OrderStatus.Recieved).FirstOrDefault();
                if (orderStatus != null)
                {
                    return true;
                }
            }
            return false;
        }
        //public bool UpdateOrder(int paymentId, string userId)
        //{
        //    if (paymentId != null)
        //    {
        //        var payment = _context.Payments.Where(x => x.Id == paymentId && x.PaidBy == userId  && x.PaymentVerificationStatus == PaymentVerificationStatus.Seen).Include(x => x.ApplicationsUser).Include(x => x.Orders).FirstOrDefault();
        //        if (payment != null)
        //        {
        //            payment.OrderStatus = OrderStatus.Recieved;
        //            _context.Update(payment);
        //            _context.SaveChanges();



        //            var updateUserOrder = _context.Orders.Where(x => x.UserId == userId && x.PaymentId == paymentId).Include(x => x.Payment).FirstOrDefault();
        //            if (updateUserOrder != null)
        //            {
        //                updateUserOrder.OrderStatus = OrderStatus.Recieved;
        //                _context.Update(updateUserOrder);
        //                _context.SaveChanges();
        //            }
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public bool UpdateOrder(int paymentId, string userId)
        {
            if (_context == null)
            {
                // Log or handle the case where _context is null
                // Example: Log.Error("Database context is null.");
                return false;
            }

            if (paymentId != 0 && !string.IsNullOrEmpty(userId))
            {
                var payment = _context.Payments
                    .Include(x => x.ApplicationsUser)
                    .Include(x => x.Orders)
                    .FirstOrDefault(x => x.Id == paymentId && x.PaidBy == userId && x.PaymentVerificationStatus == PaymentVerificationStatus.Seen);

                if (payment != null)
                {
                    payment.OrderStatus = OrderStatus.Recieved;
                    _context.Update(payment);
                    //_context.SaveChanges();

                    var updateUserOrder = _context.Orders.Where(x => x.UserId == userId && x.PaymentId == paymentId)
                        .FirstOrDefault();

                    if (updateUserOrder != null)
                    {
                        updateUserOrder.OrderStatus = OrderStatus.Recieved;
                        _context.Update(updateUserOrder);
                        _context.SaveChanges();
                    }
                    else
                    {
                        // Log or handle the case where no matching order was found
                        // Example: Log.Information("No matching order found for UserId: {userId} and PaymentId: {paymentId}", userId, paymentId);
                        return false;
                    }

                    return true;
                }
                else
                {
                    // Log or handle the case where no matching payment was found
                    // Example: Log.Information("No matching payment found for PaymentId: {paymentId} and UserId: {userId}", paymentId, userId);
                    return false;
                }
            }
            else
            {
                // Log or handle the case where paymentId or userId is invalid
                // Example: Log.Warning("Invalid paymentId or userId.");
                return false;
            }
        }

        public bool UpdateOrderToCompleted(int paymentId, string userId)
        {
            if (paymentId != null)
            {
                var payment = _context.Payments.Where(x => x.Id == paymentId && x.PaymentVerificationStatus == PaymentVerificationStatus.Seen && x.OrderStatus == OrderStatus.Recieved ).Include(x => x.ApplicationsUser).Include(x => x.Orders).FirstOrDefault();
                if (payment != null)
                {
                    payment.PaymentVerificationStatus = PaymentVerificationStatus.Completed;
                    payment.OrderStatus = OrderStatus.Completed;
                    _context.Update(payment);
                    _context.SaveChanges();



                    var updateUserOrder = _context.Orders.Where(x =>/* x.UserId == userId*/  x.OrderStatus == OrderStatus.Recieved && x.Payment.Id == paymentId).FirstOrDefault();
                    if (updateUserOrder != null)
                    {
                        updateUserOrder.PaymentVerificationStatus = PaymentVerificationStatus.Completed;
                        updateUserOrder.OrderStatus = OrderStatus.Completed;
                        _context.Update(updateUserOrder);
                        _context.SaveChanges();
                    }
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
