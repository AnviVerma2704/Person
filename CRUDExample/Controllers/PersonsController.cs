using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilters;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
  public class PersonsController : Controller
  {
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonsGetterService personsGetterService, IPersonsAdderService personsAdderService, IPersonsDeleterService personsDeleterService, IPersonsUpdaterService personsUpdaterService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsGetterService = personsGetterService;
            _personsAdderService = personsAdderService;
            _personsUpdaterService = personsUpdaterService;
            _personsDeleterService = personsDeleterService;

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
        List<PersonResponse> person = _personsGetterService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchBy = searchBy;

            List<PersonResponse> sortedPersons = _personsGetterService.GetSortedPersons(person, sortBy, sortOrder);
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortBy = sortBy;
            return View(sortedPersons);
        }
        [Route("persons/create")]
        [HttpGet]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "key", "value" })]
        [TypeFilter(typeof(TokenResultFilter))]

        public IActionResult Create()
        {
            ViewBag.Countries = _countriesService.GetAllCountries();
            return View();
        }
        [Route("persons/create")]
        [HttpPost]
        [TypeFilter(typeof(TokenAuthorizationFilter))]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return View();
            }
            _personsAdderService.AddPerson(personAddRequest);
            return RedirectToAction("Index", "Persons");
        }

        [Route("persons/pdf")]
        public async Task<IActionResult> PersonPdf()
        {
            List<PersonResponse> persons = _personsGetterService.GetAllPersons();
            return new ViewAsPdf("PersonsPdf", persons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 20, 20, 20),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            };
        }
        [Route("persons/csv")]
        public async Task<IActionResult> PersonCSV()
        {
            MemoryStream memoryStream = await _personsGetterService.GetPersonsCSV();
            return File(memoryStream, "text/octet-stream", "Persons.csv");
        }
        [Route("persons/excel")]
        public async Task<IActionResult> PersonExcel()
        {
            MemoryStream memoryStream = await _personsGetterService.GetPersonsCSV();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }
    }
}
