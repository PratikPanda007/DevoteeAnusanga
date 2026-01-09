using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using DevoteeAnusanga.Helper;
using DevoteeAnusanga.Models.Common;
using DevoteeAnusanga.Models.Admin;
using DevoteeAnusanga.Models.Announcement;

namespace DevoteeAnusanga.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        // ===============================
        // DASHBOARD STATS
        // ===============================
        [HttpGet("stats")]
        public IActionResult GetDashboardStats()
        {
            if (!IsCurrentUserAdmin())
                return Forbid();

            using var conn = DBUtils.GetConnection();
            conn.Open();

            var stats = new AdminDashboardStats();

            using (var cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM devotees_profiles) AS TotalMembers,
                    (SELECT COUNT(*) FROM devotees_profiles WHERE is_public = 1) AS PublicMembers,
                    (SELECT COUNT(*) FROM devotees_profiles WHERE is_public = 0) AS PrivateMembers,
                    (SELECT COUNT(*) FROM devotees_announcements) AS TotalAnnouncements,
                    (SELECT COUNT(*) FROM devotees_announcements WHERE status = 'pending') AS PendingAnnouncements,
                    (SELECT COUNT(*) FROM devotees_announcements WHERE status = 'approved') AS ApprovedAnnouncements,
                    (SELECT COUNT(*) FROM devotees_announcements WHERE status = 'rejected') AS RejectedAnnouncements,
                    (SELECT COUNT(*) FROM devotees_countries) AS TotalCountries,
                    (SELECT COUNT(*) FROM devotees_users) AS TotalUsers
            ", conn))
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    stats.TotalMembers = reader.GetInt32(0);
                    stats.PublicMembers = reader.GetInt32(1);
                    stats.PrivateMembers = reader.GetInt32(2);
                    stats.TotalAnnouncements = reader.GetInt32(3);
                    stats.PendingAnnouncements = reader.GetInt32(4);
                    stats.ApprovedAnnouncements = reader.GetInt32(5);
                    stats.RejectedAnnouncements = reader.GetInt32(6);
                    stats.TotalCountries = reader.GetInt32(7);
                    stats.TotalUsers = reader.GetInt32(8);
                }
            }

            return Ok(ApiResponse<AdminDashboardStats>.Success(stats));
        }

        // ===============================
        // GET ALL ANNOUNCEMENTS
        // ===============================
        [HttpGet("announcements")]
        public IActionResult GetAnnouncements(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? status = null
        )
        {
            if (!IsCurrentUserAdmin())
                return Forbid();

            var results = new List<object>();
            int total = 0;

            using var conn = DBUtils.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("sp_admin_get_announcements", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@page", page);
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@status", (object?)status ?? DBNull.Value);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                results.Add(new
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Category = reader.GetString(2),
                    Status = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    AuthorName = reader.GetString(5)
                });
            }

            if (reader.NextResult() && reader.Read())
                total = reader.GetInt32(0);

            return Ok(new PaginatedResponse<object>
            {
                Success = true,
                Data = results,
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            });
        }

        // ===============================
        // UPDATE ANNOUNCEMENT STATUS
        // ===============================
        [HttpPut("announcements/{id}/status")]
        public IActionResult UpdateAnnouncementStatus(Guid id, [FromBody] UpdateAnnouncementStatusRequest request)
        {
            if (!IsCurrentUserAdmin())
                return Forbid();

            using var conn = DBUtils.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE devotees_announcements
                SET status = @status,
                    admin_notes = @notes,
                    updated_at = GETUTCDATE()
                WHERE id = @id
            ", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@status", request.Status.ToLower());
            cmd.Parameters.AddWithValue("@notes", (object?)request.AdminNotes ?? DBNull.Value);

            var rows = cmd.ExecuteNonQuery();
            if (rows == 0)
                return NotFound(ApiResponse.Fail("Announcement not found"));

            return Ok(ApiResponse.Success("Status updated"));
        }

        // ===============================
        // DELETE ANNOUNCEMENT
        // ===============================
        [HttpDelete("announcements/{id}")]
        public IActionResult DeleteAnnouncement(Guid id)
        {
            if (!IsCurrentUserAdmin())
                return Forbid();

            using var conn = DBUtils.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(
                "DELETE FROM devotees_announcements WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            var rows = cmd.ExecuteNonQuery();
            if (rows == 0)
                return NotFound(ApiResponse.Fail("Announcement not found"));

            return Ok(ApiResponse.Success("Deleted"));
        }

        // ===============================
        // ADMIN CHECK
        // ===============================
        private bool IsCurrentUserAdmin()
        {
            var roleClaim = User.FindFirst("role_id")?.Value;
            return int.TryParse(roleClaim, out int roleId) && roleId == 3;
        }
    }
}
