// Models/Admin/AdminModels.cs
using System.ComponentModel.DataAnnotations;

namespace DevoteeAnusanga.Models.Admin
{
    // PUT /api/admin/profiles/{id}/role - Update user role
    public class UpdateUserRoleRequest
    {
        [Required]
        [Range(1, 3)]
        public int RoleId { get; set; }
    }

    // Admin dashboard stats
    public class AdminDashboardStats
    {
        // Member Statistics
        public int TotalMembers { get; set; }
        public int PublicMembers { get; set; }
        public int PrivateMembers { get; set; }
        public int NewMembersThisMonth { get; set; }
        public int NewMembersThisWeek { get; set; }

        // Announcement Statistics
        public int TotalAnnouncements { get; set; }
        public int PendingAnnouncements { get; set; }
        public int ApprovedAnnouncements { get; set; }
        public int RejectedAnnouncements { get; set; }
        public int NewAnnouncementsThisMonth { get; set; }
        public int NewAnnouncementsThisWeek { get; set; }

        // Reference Data Statistics
        public int TotalCountries { get; set; }
        public int TotalUsers { get; set; }  // Auth users count

        // Breakdown Statistics (for charts/analytics)
        public Dictionary<string, int> MembersByCountry { get; set; } = new();
        public Dictionary<string, int> AnnouncementsByCategory { get; set; } = new();
        public Dictionary<string, int> AnnouncementsByStatus { get; set; } = new();

        // Recent Activity
        public List<RecentActivityItem> RecentActivity { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string Type { get; set; } = string.Empty;  // "member_joined", "announcement_created", "announcement_approved"
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Guid? RelatedId { get; set; }
    }


    // GET /api/admin/users - Admin user list
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool HasProfile { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    // Admin user query params
    public class AdminUserQueryParams
    {
        public string? Search { get; set; }
        public int? RoleId { get; set; }
        public bool? HasProfile { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
