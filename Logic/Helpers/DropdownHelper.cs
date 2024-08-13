using Core.Data;
using Core.Models;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
    public class DropdownHelper : IDropdownHelper
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly IUserHelper _userHelper;
        public DropdownHelper(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserHelper userHelper)
        {
            _context = context;
            _userManager = userManager;
            _userHelper = userHelper;
        }

        public async Task<List<State>> GetState()
        {
            var common = new State()
            {
                Id = 0,
                Name = "-- Select --"

            };
            var selectedBranches = await _context.States.OrderBy(x => x.Name).Where(x => x.Active && !x.Deleted).ToListAsync();
            if (selectedBranches != null)
            {
                selectedBranches.Insert(0, common);
                return selectedBranches;
            }
            return null;
        }
    }
}
