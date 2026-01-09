// Models/Profile/ProfileModels.cs
using System.ComponentModel.DataAnnotations;

namespace DevoteeAnusanga.Models.Profile
{
    // Social links JSON structure
    public class SocialLinksDto
    {
        public string? Website { get; set; }
        public string? Linkedin { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Twitter { get; set; }
    }

    // Full profile DTO (GET responses)
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Country { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? MissionDescription { get; set; }
        public string? AvatarUrl { get; set; }
        public SocialLinksDto? SocialLinks { get; set; }
        public bool IsPublic { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime? AgreedToTermsAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // POST /api/profiles - Create profile
    public class CreateProfileRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1000)]
        public string? MissionDescription { get; set; }

        public SocialLinksDto? SocialLinks { get; set; }

        public bool IsPublic { get; set; } = true;
    }

    // PUT /api/profiles/{id} - Update profile
    public class UpdateProfileRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1000)]
        public string? MissionDescription { get; set; }

        public SocialLinksDto? SocialLinks { get; set; }

        public bool? IsPublic { get; set; }
    }

    // GET /api/profiles (list) - Query parameters
    public class ProfileQueryParams
    {
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
    }

    // Profile list item (for directory)
    public class ProfileListItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string Country { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? MissionDescription { get; set; }
        public string? AvatarUrl { get; set; }
        public SocialLinksDto? SocialLinks { get; set; }
        public int RoleId { get; set; }
    }

    // GET /api/profiles/me - Current user's profile response
    public class MyProfileResponse
    {
        public bool Success { get; set; }
        public bool HasProfile { get; set; }
        public ProfileDto? Profile { get; set; }
    }

    // DELETE /api/profiles/{id} - requires name confirmation
    public class DeleteProfileRequest
    {
        [Required]
        public string ConfirmName { get; set; } = string.Empty;
    }
}
