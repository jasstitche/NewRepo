
using Core.Data;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; 


namespace eFashion.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        private readonly IUserHelper _userHelper;
        public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,ApplicationDbContext context,IUserHelper userHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userHelper = userHelper;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AdminRegister()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AdminRegister(ApplicationUserViewModel applicationUserViewModel)
        {
            if (ModelState.IsValid)
            {
                if (applicationUserViewModel.Password != applicationUserViewModel.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "The password and confirm password do not match.");
                    return View(applicationUserViewModel);
                }

            }
            var checkForEmail = _context.ApplicationUsers.Where(x => x.Email == applicationUserViewModel.Email).FirstOrDefault();
            if (checkForEmail != null)
            {
                return StatusCode(500, "Email already Exist");
            }
            if (applicationUserViewModel != null)
            {
                var userEmail = await _userManager.FindByEmailAsync(applicationUserViewModel.Email);
                applicationUserViewModel.Role = "Admin";
                var addUser = await _userHelper.CreateUserByAsync(applicationUserViewModel);
                if (addUser != null)
                {
                    await _signInManager.PasswordSignInAsync(addUser, addUser.PasswordHash, true, true);

                    return RedirectToAction("Login", "Account");
                }
            }
            return View(applicationUserViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Register(ApplicationUserViewModel applicationUserViewModel)
        {
            if (ModelState.IsValid)
            {
                if (applicationUserViewModel.Password != applicationUserViewModel.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "The password and confirm password do not match.");
                    return View(applicationUserViewModel);
                }

            }
            var checkForEmail = _context.ApplicationUsers.Where(x => x.Email == applicationUserViewModel.Email).FirstOrDefault();
            if (checkForEmail != null)
            {
                return StatusCode(500, "Email already Exist");
            }
            if (applicationUserViewModel != null)
            {
                var userEmail = await _userManager.FindByEmailAsync(applicationUserViewModel.Email);
                applicationUserViewModel.Role = "User";
                var addUser = await _userHelper.CreateUserByAsync(applicationUserViewModel);
                if (addUser != null)
                {
                    await _signInManager.PasswordSignInAsync(addUser, addUser.PasswordHash, true, true);

                    return RedirectToAction("Login", "Account");
                }
            }
            return View(applicationUserViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (model != null)
                {
                    var user = _userHelper.FindUserByEmail(model.Email);
                    if (user != null)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, true, true).ConfigureAwait(false);

                        if (result.Succeeded)
                        {
                            var userRole = await _userManager.GetRolesAsync(user);
                            if (userRole != null)
                            {
                                user.Role = userRole.FirstOrDefault();
                            }
                            if (user.Role?.ToLower() == "admin")
                            {
                                return RedirectToAction("AdminDashBoard", "Admin");
                            }
                            else
                            {
                                return RedirectToAction("UserIndex", "User");
                            }
                        }
                        ViewBag.Message = "Invalid Login Attempt";
                        ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");

        }
    }
}
