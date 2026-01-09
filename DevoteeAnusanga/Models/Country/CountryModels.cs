// Models/Country/CountryModels.cs
using System.ComponentModel.DataAnnotations;

namespace DevoteeAnusanga.Models.Country
{
    // Country DTO
    public class CountryDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // POST /api/admin/countries - Create country (Admin only)
    public class CreateCountryRequest
    {
        [Required]
        [StringLength(3, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }

    // PUT /api/admin/countries/{id} - Update country (Admin only)
    public class UpdateCountryRequest
    {
        [StringLength(3, MinimumLength = 2)]
        public string? Code { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }
    }

    // GET /api/countries - List response
    public class CountryListResponse
    {
        public bool Success { get; set; }
        public List<CountryDto> Countries { get; set; } = new();
    }
}
