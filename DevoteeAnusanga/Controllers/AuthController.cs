using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;
using DevoteeAnusanga.Helper;
using DevoteeAnusanga.Models.Auth;
using DevoteeAnusanga.Models.Common;
using DevoteeAnusanga.Models.Role;

namespace DevoteeAnusanga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // -----------------------------
        // POST: api/auth/register
        // -----------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request" });

            using var conn = DBUtils.GetConnection();
            await conn.OpenAsync();

            // Check if email exists
            var checkCmd = new SqlCommand(
                "SELECT COUNT(1) FROM devotees_users WHERE email = @email", conn);
            checkCmd.Parameters.AddWithValue("@email", request.Email);

            var exists = (int)await checkCmd.ExecuteScalarAsync();
            if (exists > 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email already registered"
                });
            }

            var userId = Guid.NewGuid();
            var passwordHash = PasswordHelper.Hash(request.Password);

            // Insert user
            var insertUserCmd = new SqlCommand(@"
                INSERT INTO devotees_users
                (id, email, password_hash, name, email_verified, created_at, updated_at)
                VALUES
                (@id, @email, @password, @name, 0, GETUTCDATE(), GETUTCDATE())
            ", conn);

            insertUserCmd.Parameters.AddWithValue("@id", userId);
            insertUserCmd.Parameters.AddWithValue("@email", request.Email);
            insertUserCmd.Parameters.AddWithValue("@password", passwordHash);
            insertUserCmd.Parameters.AddWithValue("@name", request.Name);

            await insertUserCmd.ExecuteNonQueryAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Registration successful. Please login."
            });
        }

        // -----------------------------
        // POST: api/auth/login
        // -----------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request" });

            using var conn = DBUtils.GetConnection();
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT 
                    u.id,
                    u.email,
                    u.password_hash,
                    u.name,
                    p.role_id
                FROM devotees_users u
                LEFT JOIN devotees_profiles p ON p.user_id = u.id
                WHERE u.email = @email
            ", conn);

            cmd.Parameters.AddWithValue("@email", request.Email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read())
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            var userId = reader.GetGuid(0);
            var email = reader.GetString(1);
            var passwordHash = reader.GetString(2);
            var name = reader.IsDBNull(3) ? "" : reader.GetString(3);
            var roleId = reader.IsDBNull(4) ? RoleConstants.Basic : reader.GetInt32(4);

            if (!PasswordHelper.Verify(request.Password, passwordHash))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            var token = GenerateJwtToken(userId, email, roleId);

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                User = new UserDto
                {
                    Id = userId,
                    Email = email,
                    Name = name,
                    RoleId = roleId,
                    RoleName = roleId == RoleConstants.Admin ? "admin" :
                               roleId == RoleConstants.Devotee ? "devotee" : "basic",
                    IsAdmin = roleId == RoleConstants.Admin,
                    IsDevotee = roleId == RoleConstants.Devotee
                }
            });
        }

        // -----------------------------
        // JWT Generator
        // -----------------------------
        private string GenerateJwtToken(Guid userId, string email, int roleId)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("role_id", roleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
