using Core.Data;
using Core.Models;
using Core.ViewModels;
using Logic.Helpers;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using static Core.Enum.eFashionEnum;

namespace eFashion.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly IAdminHelper _adminHelper;
        private readonly ICompositeViewEngine _viewEngine;

        public CartController(ApplicationDbContext context, IAdminHelper adminHelper, UserManager<ApplicationUser> userManager, ICompositeViewEngine viewEngine)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
        {
            _context = context;
            _userManager = userManager;
            _adminHelper = adminHelper;
            _viewEngine = viewEngine;

        }
        [HttpGet]
        public IActionResult Add()
        {

            return View();
        }

        [HttpPost]
        public JsonResult Add(int sampleId )
        {
            var userName  = User.Identity.Name;
            var userId = _adminHelper.GetUserId(userName);

            if (sampleId > 0)
            {
                var queryCartTable = _context.Carts.Where(x => x.SampleId == sampleId && x.UserId == userId && x.Deleted == false && x.Active == true).FirstOrDefault();
                if (queryCartTable != null)
                {
                    return Json(new { isError = true, msg = "Cart already exist, please place your order." });
                }

                var CreatedCartSample = _adminHelper.CreateCart(sampleId, userId);
                if (CreatedCartSample != null)
                {
                    var listOfCart = _adminHelper.GetAllCart(userName).Result;
                    var cartCount = listOfCart != null ? listOfCart.Count() : 0;

                    return Json(new { isError = false, msg = "Item added successfully", cartCount = cartCount });
                }
            }
            return Json(new { isError = true, msg = "Error Occurred while Creating Cart" });
        }






        //[HttpGet]
        //public IActionResult Cartistpartial()
        //{
        //    var model = _adminHelper.GetAllCart();
        //    return PartialView("_Cartlistpartial", model);
        //}

        //[HttpGet]
        //public async Task<JsonResult> ViewCartpartial(string email)
        //{
        //    var model = await _adminHelper.GetAllCart(email);
        //    if (model != null)
        //    {
        //        return Json(new { isError = false,"_Cartlistpartial", model });
        //    }
        //    return Json(new { isError = false, "_Cartlistpartial", new List<Cart>() }); // Return an empty list or appropriate fallback
        //}

        [HttpGet]
        public async Task<IActionResult> _CartListPartial(string email)
        {
            var cartItems = await _adminHelper.GetAllCart(email);
            if (cartItems != null)
            {
                IEnumerable<SamplePageViewModel> ffff = cartItems.Select(e => new SamplePageViewModel
                {   
                    
                    id = e.Id,
                    SampleId = e.SampleId,
                    AppUser = e.AppUser,
                    SamplesPage = e.SamplesPage,
                    UserId = e.UserId,
                    Quantity = e.Quantity,
                    SubTotal = e.SubTotal ?? e.SamplesPage.Price,
                    TotalAmount = e.TotalAmount
                }).ToList();
                return PartialView(ffff);

                //return Json(new { isError = false, cartItems });

            }
            return View();

            // return Json(new { isError = true });
        }



        [HttpGet]
        public async Task<IActionResult> CartPartial(string email)
        {
            if (email != null)
            {
                var cartItems = await  _adminHelper.GetAllCart(email);
                if (cartItems != null)
                {

                    return PartialView(cartItems);
                }
            }
            return View();


           
            
        }
        [HttpPost]
        public async Task<JsonResult> DeleteCart(int id)
        {
            var userName = User.Identity.Name;
            if (id != 0) 
            { 
                var cart = await _adminHelper.DeleteCartbyId(id);
                if (cart != null)
                {
                    var listOfCart = await _adminHelper.GetAllCart(userName);
                    var cartCount = listOfCart != null ? listOfCart.Count() : 0;

                    return Json(new { isError = false, msg = "Item removed successfully", cart, cartCount });
                }
                //if (cart != null) 
                //{
                //    return Json(cart);
                //}
            }            
             return Json(new { isError = true, msg = "'Failed to delete cart item" });


        }

        [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int id, int change)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var queryCartTable = _context.Carts.Where(x => x.SampleId == id && x.UserId == userId && x.Deleted == false && x.Active == true).FirstOrDefault();
            if (queryCartTable != null)
            {
                return Json(new { isError = true, msg = "Cart already exist, please place your order." });
            }
            if (id != 0)
            {
                var cart = await _adminHelper.UpdateQuantityProcess(id, change, userId);
                if (cart != null)
                {
                    return Json(cart);
                }
            }
            return Json(new { isError = true, msg = "'Failed to update cart item" });
        }


        
      
        [HttpGet]
        public IActionResult GetTotalAmount(string paymentTypeId, int stateId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cartItems = _context.Carts
                    .Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active)
                    .Include(c => c.SamplesPage)
                    .Include(c => c.AppUser)
                    .ToList();

                if (cartItems == null || !cartItems.Any())
                {
                    return Json(new { isError = true, message = "Cart is empty" });
                }

                // Retrieve the delivery fee based on the selected state
                var deliveryFee = _context.States
                    .Where(s => s.Id == stateId)
                    .FirstOrDefault()?.DeliveryFee ?? 0;

                var totalAmount = cartItems.Sum(c => c.SubTotal);
                var totalAmountToPay = totalAmount + deliveryFee;

                var cartItemsDetails = cartItems.Select(c => new
                {
                    c.Id,
                    c.SamplesPage.MaterialName,
                    c.Quantity,
                    c.SamplesPage.Price,
                    c.SubTotal
                }).ToList();

                var getCompanySettings = _adminHelper.GetCompanySetting().Result;

                getCompanySettings.DeliveryFee = deliveryFee.ToString();

                return Json(new
                {
                    isError = false,
                    totalAmount = totalAmount,
                    paymentTypeId = paymentTypeId,
                    totalAmountToPay = totalAmountToPay,
                    deliveryFee = deliveryFee,
                    cartItems = cartItemsDetails,
                    getCompanySettings = new
                    {
                        getCompanySettings.AccountName,
                        getCompanySettings.AccountNumber,
                        getCompanySettings.BankName,
                        getCompanySettings.CompanyAddress,
                        getCompanySettings.PickUpDays,
                        getCompanySettings.DeliveryAddress,
                        getCompanySettings.DeliveryFee,
                        
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { isError = true, message = ex.Message });
            }
        }




        //public IActionResult GetTotalAmount(String paymentTypeId)
        //{

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var cartItems = _context.Carts.Where(c => c.AppUser.Id == userId && !c.Deleted && c.Active).Include(c => c.SamplesPage).Include(c => c.AppUser).ToList();

        //    if (cartItems == null || !cartItems.Any())
        //    {
        //        return Json(new { isError = true, message = "Cart is empty" });
        //    }

        //    var getCompanySettings = _adminHelper.GetCompanySetting().Result;
        //    var deliveryFee = getCompanySettings?.DeliveryFee != null ? Convert.ToDecimal(getCompanySettings.DeliveryFee) : 0;
        //    var totalAmount = cartItems.Sum(c => c.SubTotal);
        //    var totalAmountToPay = totalAmount + deliveryFee;
        //    var cartItemsDetails = cartItems.Select(c => new
        //    {
        //        c.Id,
        //        c.SamplesPage.MaterialName,
        //        c.Quantity,
        //        c.SamplesPage.Price,
        //        c.SubTotal
        //    }).ToList();

        //    return Json(new { isError = false, totalAmount = totalAmount ,
        //        paymentTypeId = paymentTypeId ,
        //        totalAmountToPay = totalAmountToPay, cartItems = cartItemsDetails,
        //        getCompanySettings = new
        //        {
        //            getCompanySettings.AccountName,
        //            getCompanySettings.AccountNumber, 
        //            getCompanySettings.BankName ,
        //            getCompanySettings.CompanyAddress,
        //            getCompanySettings.PickUpDays,
        //            getCompanySettings.DeliveryAddress,
        //            getCompanySettings.DeliveryFee,


        //        }
        //    });
        //}

    }

}   







