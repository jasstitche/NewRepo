using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Logic.Helpers.UserHelper;

namespace Logic.IHelpers
{
    public interface IUserHelper
    {
        Task<ApplicationUser> CreateUserByAsync(ApplicationUserViewModel applicationUserViewModel);
        ApplicationUser FindUserByEmail(string email);
        List<DropdownEnumModel> GetDropDownEnumsList();
        Task<bool> SavePaymentDetails(OrdersViewModel ordersViewModel, string userId, string unqueFileName);
        bool CheckIfPendingPayment(string userId);
        bool CheckIfApproved(int paymentId);
        bool CheckUserRegPayment(string userId);
    }
}
