using Core.Data;
using Core.Models;
using Logic.IHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eFashion.Controllers
{
    public class SamplePageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAdminHelper _adminHelper;
        public SamplePageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAdminHelper adminHelper)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _adminHelper = adminHelper;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var samples = _context.SamplePages.ToList(); // Ensure this is a collection
            return View(samples);
        }

        [HttpGet]
        public IActionResult EditSample(int id)
        {
            var sample = _context.SamplePages.Find(id);
            if (sample == null)
            {
                return NotFound();
            }
            return View(sample);
        }

        [HttpPost]
        public IActionResult EditSample(SamplePage sampleDetails)
        {
            if (sampleDetails == null)
            {
                TempData["ErrorMessage"] = "Unable to edit sample.";
                return NotFound();
            }
            if (sampleDetails.SampleImage != null)
            {
                // Upload the new image and get the file path
                string uniqueFileName = UploadedFile(sampleDetails.SampleImage);
                sampleDetails.File = uniqueFileName;
            }
            else
            {
                // Retain the existing image if no new image is uploaded
                var existingSample = _context.SamplePages.AsNoTracking().FirstOrDefault(s => s.id == sampleDetails.id);
                if (existingSample != null)
                {
                    sampleDetails.File = existingSample.File;
                }
            }

                _context.SamplePages.Update(sampleDetails);
                _context.SaveChanges();

            TempData["SuccessMessage"] = "Sample successfully updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DeleteSample(int id)
        {
            var sample = _context.SamplePages.Find(id);
            if (sample == null)
            {
                return NotFound();
            }
            return View(sample);
        }


        [HttpPost, ActionName("DeleteSample")]
        public IActionResult DeleteSampleConfirmed(int id)
        {
            var sample = _context.SamplePages.Find(id);

            if (sample == null)
            {
                TempData["ErrorMessage"] = "Sample not found.";
                return NotFound();
            }

            // Retrieve all related orders
            var relatedOrders = _context.Orders.Where(o => o.SampleId == id).ToList();

            // Remove Payment references from Orders first
            foreach (var order in relatedOrders)
            {
                // Remove Payment references from Orders (assuming nullable foreign key)
                order.PaymentId = null; // Or set to a default value if necessary
                _context.Orders.Update(order);
            }

            // Save changes to remove PaymentId references
            _context.SaveChanges();

            // Now delete related payments
            foreach (var order in relatedOrders)
            {
                var relatedPayments = _context.Payments.Where(p => p.OrdersId == order.Id).ToList();
                _context.Payments.RemoveRange(relatedPayments);
            }

            // Delete related orders
            _context.Orders.RemoveRange(relatedOrders);

            // Delete related carts
            var relatedCarts = _context.Carts.Where(c => c.SampleId == id).ToList();
            _context.Carts.RemoveRange(relatedCarts);

            // Delete the sample record
            _context.SamplePages.Remove(sample);

            // Save changes to the database
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sample successfully deleted!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CreateSample()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateSample(SamplePage sampleDetails)
        { 

                if (sampleDetails.DesignName == null)
                {
                    return View(sampleDetails);
                }
                if (sampleDetails.MaterialName == null)
                {
                    return View(sampleDetails);
                }
                if (sampleDetails.ClothSize == null)
                {
                    return View(sampleDetails);
                }
                if (sampleDetails.Price == null) 
                {
                    return View(sampleDetails);
                }
                if (sampleDetails.SampleImage == null)
                {
                    return View(sampleDetails);
                }
                 if (sampleDetails.NumberOfItem == null)
                {
                return View(sampleDetails);
                }

            string unqueFileName = String.Empty;
            if (sampleDetails.SampleImage != null)
            {
                unqueFileName = UploadedFile(sampleDetails.SampleImage);
            }
            
            var saveSampleDetails = _adminHelper.CreateSample(sampleDetails,unqueFileName);
            if (saveSampleDetails != null) 
            {
                //you should return a redirect to display the samples created
                return RedirectToAction("Shop", "Shop");
                //return View();
            }


            return View("Error");
           
        }

        public string UploadedFile(IFormFile filesSender)
        {
            string uniqueFileName = string.Empty;

            if (filesSender != null && filesSender.Length >0)
            {
                // Define the path to the folder where files will be uploaded
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "sampleupload");

                // Create the folder if it does not exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name using a GUID
                uniqueFileName = Guid.NewGuid().ToString() + "_" + filesSender.FileName;

                // Combine the folder path and the unique file name to get the full path
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    filesSender.CopyTo(fileStream);
                }

                // Return the relative path to the file (this will be used in the view)
                return Path.Combine("sampleupload", uniqueFileName).Replace("\\","/");
            }

            return null;
        }

           }
   
}
