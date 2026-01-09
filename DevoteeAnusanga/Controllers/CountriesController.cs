using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DevoteeAnusanga.Models;
using DevoteeAnusanga.Data;
using DevoteeAnusanga.Entities;
using DevoteeAnusanga.Models.Common;
using DevoteeAnusanga.Models.Country;
using DevoteeAnusanga.Models.Role;

namespace DevoteeAnusanga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CountriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/countries
        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _context.Countries
                .OrderBy(c => c.Name)
                .Select(c => new CountryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(new ApiResponse<CountryListResponse>
            {
                Success = true,
                Data = new CountryListResponse
                {
                    Countries = countries,
                    Total = countries.Count
                }
            });
        }

        // GET: api/countries/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCountry(Guid id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Country not found"
                });
            }

            return Ok(new ApiResponse<CountryDto>
            {
                Success = true,
                Data = new CountryDto
                {
                    Id = country.Id,
                    Code = country.Code,
                    Name = country.Name
                }
            });
        }

        // POST: api/countries (Admin only)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request data" });

            // Check if country code already exists
            var existingCountry = await _context.Countries
                .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpper());

            if (existingCountry != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Country code already exists"
                });
            }

            var country = new Country
            {
                Id = Guid.NewGuid(),
                Code = request.Code.ToUpper(),
                Name = request.Name
            };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, new ApiResponse<CountryDto>
            {
                Success = true,
                Message = "Country created",
                Data = new CountryDto
                {
                    Id = country.Id,
                    Code = country.Code,
                    Name = country.Name
                }
            });
        }

        // PUT: api/countries/{id} (Admin only)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCountry(Guid id, [FromBody] UpdateCountryRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Country not found"
                });
            }

            if (request.Code != null) country.Code = request.Code.ToUpper();
            if (request.Name != null) country.Name = request.Name;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<CountryDto>
            {
                Success = true,
                Message = "Country updated",
                Data = new CountryDto
                {
                    Id = country.Id,
                    Code = country.Code,
                    Name = country.Name
                }
            });
        }

        // DELETE: api/countries/{id} (Admin only)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(Guid id)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Country not found"
                });
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Country deleted"
            });
        }

        private bool IsCurrentUserAdmin()
        {
            var roleIdClaim = User.FindFirst("role_id")?.Value;
            return roleIdClaim != null && int.Parse(roleIdClaim) == RoleConstants.Admin;
        }
    }
}
