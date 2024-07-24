using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eFashion.Controllers
{
    public class UserController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {

            _userManager = userManager;

        }

        [HttpGet]
        public IActionResult UserIndex()
        {

            var userName = User.Identity.Name;
            var applicationUser = _userManager.FindByNameAsync(userName).Result;
            return View(applicationUser);
        }

    }
}
