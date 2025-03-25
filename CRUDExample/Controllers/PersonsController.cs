using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
  public class PersonsController : Controller
  {
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;
        public PersonsController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;
        }
        [Route("persons/index")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder=SortOrderOptions.ASC)
        {
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
   }
}
