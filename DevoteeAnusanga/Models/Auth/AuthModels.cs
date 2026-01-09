// Models/Auth/AuthModels.cs
using System.ComponentModel.DataAnnotations;

namespace DevoteeAnusanga.Models.Auth
{
    // POST /api/auth/register
    public class RegisterRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public bool AgreedToTerms { get; set; }

        [Required]
        public bool AgreedToGuidelines { get; set; }
    }

    // POST /api/auth/login
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // Response for login/register
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }

    // User info returned in auth response
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool HasProfile { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDevotee { get; set; }
    }

    // POST /api/auth/refresh
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    // POST /api/auth/forgot-password
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    // POST /api/auth/reset-password
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // GET /api/auth/me (current user)
    public class CurrentUserResponse
    {
        public bool Success { get; set; }
        public UserDto? User { get; set; }
        //public ProfileDto? Profile { get; set; }
    }
}
