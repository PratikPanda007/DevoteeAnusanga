using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevoteeAnusanga.Models;
using DevoteeAnusanga.Data;
using DevoteeAnusanga.Models.Common;
using DevoteeAnusanga.Models.Role;

namespace DevoteeAnusanga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Id)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();

            return Ok(new ApiResponse<RoleListResponse>
            {
                Success = true,
                Data = new RoleListResponse
                {
                    Roles = roles
                }
            });
        }
    }
}
