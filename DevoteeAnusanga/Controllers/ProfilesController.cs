using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using DevoteeAnusanga.Models;
using DevoteeAnusanga.Data;
using DevoteeAnusanga.Entities;
using DevoteeAnusanga.Models.Common;
using DevoteeAnusanga.Models.Profile;
using DevoteeAnusanga.Models.Role;

namespace DevoteeAnusanga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/profiles
        [HttpGet]
        public async Task<IActionResult> GetProfiles([FromQuery] ProfileQueryParams queryParams)
        {
            var query = _context.Profiles.AsQueryable();

            // Filter by public profiles for unauthenticated users
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                query = query.Where(p => p.IsPublic == true);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(queryParams.Country))
            {
                query = query.Where(p => p.Country == queryParams.Country);
            }

            if (!string.IsNullOrEmpty(queryParams.City))
            {
                query = query.Where(p => p.City == queryParams.City);
            }

            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchLower = queryParams.Search.ToLower();
                query = query.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(searchLower)) ||
                    (p.City != null && p.City.ToLower().Contains(searchLower)));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var page = queryParams.Page ?? 1;
            var limit = queryParams.Limit ?? 50;

            var profiles = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(p => new ProfileListItemDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Name = p.Name,
                    Country = p.Country,
                    City = p.City,
                    AvatarUrl = p.AvatarUrl,
                    MissionDescription = p.MissionDescription,
                    IsPublic = p.IsPublic
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<ProfileListItemDto>
            {
                Success = true,
                Data = profiles,
                Total = totalCount,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            });
        }

        // GET: api/profiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Profile not found"
                });
            }

            // Check if profile is public or user is authenticated
            if (profile.IsPublic != true && (!User.Identity?.IsAuthenticated ?? true))
            {
                return Forbid();
            }

            return Ok(new ApiResponse<ProfileDto>
            {
                Success = true,
                Data = MapToProfileDto(profile)
            });
        }

        // GET: api/profiles/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Profile not found. Please create a profile."
                });
            }

            return Ok(new ApiResponse<MyProfileResponse>
            {
                Success = true,
                Data = new MyProfileResponse
                {
                    Profile = MapToProfileDto(profile),
                    IsComplete = IsProfileComplete(profile)
                }
            });
        }

        // POST: api/profiles
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request data" });

            var userId = GetCurrentUserId();

            // Check if profile already exists
            var existingProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Profile already exists. Use PUT to update."
                });
            }

            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Country = request.Country,
                City = request.City,
                MissionDescription = request.MissionDescription,
                SocialLinks = request.SocialLinks != null
                    ? System.Text.Json.JsonSerializer.Serialize(request.SocialLinks)
                    : null,
                IsPublic = request.IsPublic ?? true,
                RoleId = RoleConstants.Devotee, // Default role
                AgreedToTermsAt = request.AgreedToTerms ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, new ApiResponse<ProfileDto>
            {
                Success = true,
                Message = "Profile created successfully",
                Data = MapToProfileDto(profile)
            });
        }

        // PUT: api/profiles/me
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request data" });

            var userId = GetCurrentUserId();
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Profile not found"
                });
            }

            // Update only provided fields
            if (request.Name != null) profile.Name = request.Name;
            if (request.Email != null) profile.Email = request.Email;
            if (request.Phone != null) profile.Phone = request.Phone;
            if (request.Country != null) profile.Country = request.Country;
            if (request.City != null) profile.City = request.City;
            if (request.MissionDescription != null) profile.MissionDescription = request.MissionDescription;
            if (request.SocialLinks != null)
                profile.SocialLinks = System.Text.Json.JsonSerializer.Serialize(request.SocialLinks);
            if (request.IsPublic.HasValue) profile.IsPublic = request.IsPublic.Value;
            if (request.AvatarUrl != null) profile.AvatarUrl = request.AvatarUrl;

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<ProfileDto>
            {
                Success = true,
                Message = "Profile updated successfully",
                Data = MapToProfileDto(profile)
            });
        }

        // GET: api/profiles/countries
        [HttpGet("countries")]
        public async Task<IActionResult> GetProfileCountries()
        {
            var countries = await _context.Profiles
                .Where(p => p.IsPublic == true)
                .Select(p => p.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Data = countries
            });
        }

        // GET: api/profiles/cities
        [HttpGet("cities")]
        public async Task<IActionResult> GetProfileCities([FromQuery] string? country)
        {
            var query = _context.Profiles.Where(p => p.IsPublic == true);

            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(p => p.Country == country);
            }

            var cities = await query
                .Where(p => p.City != null)
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(new ApiResponse<List<string?>>
            {
                Success = true,
                Data = cities
            });
        }

        // Helper methods
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        private ProfileDto MapToProfileDto(Profile profile)
        {
            return new ProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Name = profile.Name,
                Email = profile.Email,
                Phone = profile.Phone,
                Country = profile.Country,
                City = profile.City,
                MissionDescription = profile.MissionDescription,
                AvatarUrl = profile.AvatarUrl,
                SocialLinks = profile.SocialLinks != null
                    ? System.Text.Json.JsonSerializer.Deserialize<SocialLinksDto>(profile.SocialLinks)
                    : null,
                IsPublic = profile.IsPublic,
                RoleId = profile.RoleId,
                AgreedToTermsAt = profile.AgreedToTermsAt,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        private bool IsProfileComplete(Profile profile)
        {
            return !string.IsNullOrEmpty(profile.Name) &&
                   !string.IsNullOrEmpty(profile.Country) &&
                   profile.AgreedToTermsAt != null;
        }
    }
}
