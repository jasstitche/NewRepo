﻿using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Logic.IHelpers
{
    public interface IAdminHelper
    {
        string GetUserId(string username);
        string GetCurrentUserId(string username);
        ApplicationUser FindByUserName(string username);
        SamplePage CreateSample(SamplePage samplePage, string unqueFileName);
        Cart CreateCart(int sampleId, string loggedInUser);
        Task<List<Cart>> GetAllCart(string email);
        Task<Cart> DeleteCartbyId(int id);
        // Task<Cart> UpdateQuantityProcess(int id, int change);
        Task<Cart> UpdateQuantityProcess(int id, int change, string userId);
        bool ApproveOrderPayment(int paymentId, string loggedInUser);
        Task<CompanySettingViewModel> GetCompanySetting();
        CompanySettingViewModel UpdateCompanySettings(CompanySettingViewModel companySettingViewModel);
        bool DeclineOrderPayment(int paymentId, string loggedInUser);
        bool CheckIfDeclined(int paymentId);
        Task<ApplicationUser> CreateAdminByAsync(ApplicationUserViewModel applicationUserViewModel);
        //StateViewModel UpdateStates(StateViewModel stateViewMode);
        Task<State> UpdateStates(StateViewModel viewModel);
        List<StateViewModel> GetAllStates();
        State GetDeliveryFeeByStateId(int stateId);
        //bool SaveState(StateViewModel model);
    }
}
