// Models/Role/RoleModels.cs
namespace DevoteeAnusanga.Models.Role
{
    // Role DTO
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // GET /api/roles - List response
    public class RoleListResponse
    {
        public bool Success { get; set; }
        public List<RoleDto> Roles { get; set; } = new();
    }

    // Role constants (matching your TypeScript ROLE_IDS)
    public static class RoleConstants
    {
        public const int Basic = 1;
        public const int Devotee = 2;
        public const int Admin = 3;
    }
}
