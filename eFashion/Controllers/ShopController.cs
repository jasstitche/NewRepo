using Core.Data;
using Core.Models;
using Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eFashion.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ShopController(ApplicationDbContext context) 
        {
            _context = context;
        }
        public IActionResult Shop()
        
        {
            var samplePages = _context.SamplePages.Where(x => x.id > 0 && x.File != null).ToList();

            // Map to ViewModel
            IEnumerable<SamplePageViewModel> samplePageViewModels = samplePages.Select(sp => new SamplePageViewModel
            {
                SampleId = sp.id,
                DesignName = sp.DesignName,
                MaterialName = sp.MaterialName,
                ClothSize = sp.ClothSize,
                Price = sp.Price ?? 0,  // Handle potential null values
                File = sp.File,
                DateSampled = sp.DateSampled,
                NumberOfItems = sp.NumberOfItem,
            }).ToList();

            //return View(samplePageViewModels);
            //IEnumerable<SamplePage> samplePages = _context.SamplePages.ToList();

            return View(samplePageViewModels);
        }
    }
}
