// Entities/User.cs
using DevoteeAnusanga.Models.Announcement;

namespace DevoteeAnusanga.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public Profile? Profile { get; set; }
    }

    public class Profile
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
        public string? SocialLinks { get; set; } // JSON string
        public bool IsPublic { get; set; } = true;
        public int RoleId { get; set; }
        public DateTime? AgreedToTermsAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public User? User { get; set; }
        public Role? Role { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Profile> Profiles { get; set; } = new List<Profile>();
    }

    public class Country
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class Announcement
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public AnnouncementStatus Status { get; set; } = AnnouncementStatus.Pending;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
