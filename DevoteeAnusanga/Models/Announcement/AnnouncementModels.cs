// Models/Announcement/AnnouncementModels.cs
using System.ComponentModel.DataAnnotations;

namespace DevoteeAnusanga.Models.Announcement
{
    // Enum matching database
    public enum AnnouncementStatus
    {
        Pending,
        Approved,
        Rejected
    }

    // Full announcement DTO
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public AnnouncementStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Joined profile data
        public AnnouncementAuthorDto? Author { get; set; }
    }

    // Author info for announcements
    public class AnnouncementAuthorDto
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Country { get; set; }
    }

    // POST /api/announcements - Create announcement
    public class CreateAnnouncementRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
    }

    // PUT /api/announcements/{id} - Update own announcement
    public class UpdateAnnouncementRequest
    {
        [StringLength(200, MinimumLength = 5)]
        public string? Title { get; set; }

        [StringLength(5000, MinimumLength = 10)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }

    // PUT /api/admin/announcements/{id}/status - Admin status update
    public class UpdateAnnouncementStatusRequest
    {
        [Required]
        public AnnouncementStatus Status { get; set; }

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // GET /api/announcements - Query parameters
    public class AnnouncementQueryParams
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public AnnouncementStatus? Status { get; set; }
        public string SortOrder { get; set; } = "latest"; // "latest" or "oldest"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // GET /api/admin/announcements/pending - Admin view
    public class AdminAnnouncementQueryParams
    {
        public AnnouncementStatus? Status { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Announcement list item
    public class AnnouncementListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public AnnouncementStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public AnnouncementAuthorDto? Author { get; set; }
    }

    // GET /api/announcements/my - User's own announcements
    public class MyAnnouncementsResponse
    {
        public bool Success { get; set; }
        public List<AnnouncementDto> Announcements { get; set; } = new();
    }
}
