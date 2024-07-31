using Core.Data;
using Core.Models;
using Logic.IHelpers;
using Microsoft.AspNetCore.Mvc;

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
