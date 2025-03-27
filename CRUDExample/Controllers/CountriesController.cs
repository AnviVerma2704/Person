using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers
{
    public class CountriesController : Controller
    {
        private readonly ICountriesService _countriesService;

        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }


        [Route("countries/UploadFromExcel")]
        public IActionResult UploadFromExcel()
        {
            return View();
        }


        [HttpPost]
        [Route("countries/UploadFromExcel")]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Please select an xlsx file";
                return View();
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
                return View();
            }

            int countriesCountInserted = await _countriesService.UploadCountriesFromExcel(excelFile);

            ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";
            return View();
        }
    }
}
