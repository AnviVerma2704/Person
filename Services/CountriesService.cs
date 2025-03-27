using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
  public class CountriesService : ICountriesService
  {
    //private field
    private readonly PersonsDbContext _db;

    //constructor
    public CountriesService(PersonsDbContext personsDbContext, bool initialize = true)
    {
            _db = personsDbContext;
    }

    public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
    {

      //Validation: countryAddRequest parameter can't be null
      if (countryAddRequest == null)
      {
        throw new ArgumentNullException(nameof(countryAddRequest));
      }

      //Validation: CountryName can't be null
      if (countryAddRequest.CountryName == null)
      {
        throw new ArgumentException(nameof(countryAddRequest.CountryName));
      }

      //Validation: CountryName can't be duplicate
      if (_db.Countries.Count(temp => temp.CountryName == countryAddRequest.CountryName) > 0)
      {
        throw new ArgumentException("Given country name already exists");
      }

      //Convert object from CountryAddRequest to Country type
      Country country = countryAddRequest.ToCountry();

      //generate CountryID
      country.CountryID = Guid.NewGuid();

      //Add country object into _countries
      _db.Countries.Add(country);
      _db.SaveChanges();

            return country.ToCountryResponse();
    }

    public List<CountryResponse> GetAllCountries()
    {
      return _db.Countries.Select(country => country.ToCountryResponse()).ToList();
    }

    public CountryResponse? GetCountryByCountryID(Guid? countryID)
    {
      if (countryID == null)
        return null;

      Country? country_response_from_list = _db.Countries.FirstOrDefault(temp => temp.CountryID == countryID);

      if (country_response_from_list == null)
        return null;

      return country_response_from_list.ToCountryResponse();
    }

        public async Task<int> UploadCountriesFromExcel(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            int countriesInserted = 0;

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = workSheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(workSheet.Cells[row, 1].Value);

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;

                        if (_db.Countries.Where(temp => temp.CountryName == countryName).Count() == 0)
                        {
                            Country country = new Country() { CountryName = countryName };
                            _db.Countries.Add(country);
                            await _db.SaveChangesAsync();

                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}

