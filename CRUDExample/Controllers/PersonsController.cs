using CRUDExample.Filters.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
  public class PersonsController : Controller
  {
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }
        [Route("persons/index")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        [TypeFilter(typeof(ResponseHeaderActionFilter),Arguments = new object[] { "X-Custom-Key","Custom-Valu" })]
        public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder=SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index action method of PersonsController");

            _logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}");
            ViewBag.SearchField = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.PersonName), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                { nameof(PersonResponse.Gender),"Gender" }
            };
        List<PersonResponse> person = _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchBy = searchBy;

            List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(person, sortBy, sortOrder);
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortBy = sortBy;
            return View(sortedPersons);
        }
        [Route("persons/create")]
        [HttpGet]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "key", "value" })]

        public IActionResult Create()
        {
            ViewBag.Countries = _countriesService.GetAllCountries();
            return View();
        }
        [Route("persons/create")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return View();
            }
            _personsService.AddPerson(personAddRequest);
            return RedirectToAction("Index", "Persons");
        }

        [Route("persons/pdf")]
        public async Task<IActionResult> PersonPdf()
        {
            List<PersonResponse> persons = _personsService.GetAllPersons();
            return new ViewAsPdf("PersonsPdf", persons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 20, 20, 20),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            };
        }
        [Route("persons/csv")]
        public async Task<IActionResult> PersonCSV()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsCSV();
            return File(memoryStream, "text/octet-stream", "Persons.csv");
        }
        [Route("persons/excel")]
        public async Task<IActionResult> PersonExcel()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsCSV();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }
    }
}
