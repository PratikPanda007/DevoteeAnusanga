using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using DevoteeAnusanga.Models;
using DevoteeAnusanga.Data;
using DevoteeAnusanga.Entities;
using DevoteeAnusanga.Models.Announcement;
using DevoteeAnusanga.Models.Common;
using DevoteeAnusanga.Models.Role;

namespace DevoteeAnusanga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/announcements
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements([FromQuery] AnnouncementQueryParams queryParams)
        {
            var query = _context.Announcements
                .Where(a => a.Status == AnnouncementStatus.Approved)
                .Include(a => a.Profile)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchLower = queryParams.Search.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchLower) ||
                    (a.Profile != null && a.Profile.Name != null &&
                     a.Profile.Name.ToLower().Contains(searchLower)));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(queryParams.Category))
            {
                query = query.Where(a => a.Category == queryParams.Category);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            var sortOrder = queryParams.SortOrder ?? "latest";
            query = sortOrder == "oldest"
                ? query.OrderBy(a => a.CreatedAt)
                : query.OrderByDescending(a => a.CreatedAt);

            // Apply pagination
            var page = queryParams.Page ?? 1;
            var limit = queryParams.Limit ?? 20;

            var announcements = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(a => new AnnouncementListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    Category = a.Category,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    Author = a.Profile != null ? new AnnouncementAuthorDto
                    {
                        Id = a.Profile.Id,
                        Name = a.Profile.Name,
                        AvatarUrl = a.Profile.AvatarUrl
                    } : null
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<AnnouncementListItemDto>
            {
                Success = true,
                Data = announcements,
                Total = totalCount,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            });
        }

        // GET: api/announcements/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncement(Guid id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Profile)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Announcement not found"
                });
            }

            // Only show approved announcements to public, or own announcements to owner
            var isOwner = User.Identity?.IsAuthenticated == true &&
                          announcement.UserId == GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (announcement.Status != AnnouncementStatus.Approved && !isOwner && !isAdmin)
            {
                return Forbid();
            }

            return Ok(new ApiResponse<AnnouncementDto>
            {
                Success = true,
                Data = MapToAnnouncementDto(announcement)
            });
        }

        // GET: api/announcements/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyAnnouncements([FromQuery] AnnouncementQueryParams queryParams)
        {
            var userId = GetCurrentUserId();

            var query = _context.Announcements
                .Where(a => a.UserId == userId)
                .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrEmpty(queryParams.Status))
            {
                if (Enum.TryParse<AnnouncementStatus>(queryParams.Status, true, out var status))
                {
                    query = query.Where(a => a.Status == status);
                }
            }

            var totalCount = await query.CountAsync();

            var announcements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    Category = a.Category,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    Author = null
                })
                .ToListAsync();

            var stats = new
            {
                Total = totalCount,
                Pending = await _context.Announcements.CountAsync(a => a.UserId == userId && a.Status == AnnouncementStatus.Pending),
                Approved = await _context.Announcements.CountAsync(a => a.UserId == userId && a.Status == AnnouncementStatus.Approved),
                Rejected = await _context.Announcements.CountAsync(a => a.UserId == userId && a.Status == AnnouncementStatus.Rejected)
            };

            return Ok(new ApiResponse<MyAnnouncementsResponse>
            {
                Success = true,
                Data = new MyAnnouncementsResponse
                {
                    Announcements = announcements,
                    Stats = stats
                }
            });
        }

        // POST: api/announcements
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request data" });

            var userId = GetCurrentUserId();

            // Check if user has a profile
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "You must create a profile before posting announcements"
                });
            }

            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                Status = AnnouncementStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // Reload with profile
            await _context.Entry(announcement).Reference(a => a.Profile).LoadAsync();

            return CreatedAtAction(nameof(GetAnnouncement), new { id = announcement.Id }, new ApiResponse<AnnouncementDto>
            {
                Success = true,
                Message = "Announcement submitted for approval",
                Data = MapToAnnouncementDto(announcement)
            });
        }

        // PUT: api/announcements/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(Guid id, [FromBody] UpdateAnnouncementRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request data" });

            var userId = GetCurrentUserId();
            var announcement = await _context.Announcements.FindAsync(id);

            if (announcement == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Announcement not found"
                });
            }

            // Only owner can update, and only if pending
            if (announcement.UserId != userId)
            {
                return Forbid();
            }

            if (announcement.Status != AnnouncementStatus.Pending)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Can only edit pending announcements"
                });
            }

            if (request.Title != null) announcement.Title = request.Title;
            if (request.Content != null) announcement.Content = request.Content;
            if (request.Category != null) announcement.Category = request.Category;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _context.Entry(announcement).Reference(a => a.Profile).LoadAsync();

            return Ok(new ApiResponse<AnnouncementDto>
            {
                Success = true,
                Message = "Announcement updated",
                Data = MapToAnnouncementDto(announcement)
            });
        }

        // DELETE: api/announcements/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(Guid id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();
            var announcement = await _context.Announcements.FindAsync(id);

            if (announcement == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Announcement not found"
                });
            }

            // Only owner or admin can delete
            if (announcement.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Announcement deleted"
            });
        }

        // GET: api/announcements/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Announcements
                .Where(a => a.Status == AnnouncementStatus.Approved)
                .Select(a => a.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Data = categories
            });
        }

        // Helper methods
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        private bool IsCurrentUserAdmin()
        {
            var roleIdClaim = User.FindFirst("role_id")?.Value;
            return roleIdClaim != null && int.Parse(roleIdClaim) == RoleConstants.Admin;
        }

        private AnnouncementDto MapToAnnouncementDto(Announcement announcement)
        {
            return new AnnouncementDto
            {
                Id = announcement.Id,
                UserId = announcement.UserId,
                Title = announcement.Title,
                Content = announcement.Content,
                Category = announcement.Category,
                Status = announcement.Status,
                AdminNotes = announcement.AdminNotes,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt,
                Author = announcement.Profile != null ? new AnnouncementAuthorDto
                {
                    Id = announcement.Profile.Id,
                    Name = announcement.Profile.Name,
                    AvatarUrl = announcement.Profile.AvatarUrl
                } : null
            };
        }
    }
}
