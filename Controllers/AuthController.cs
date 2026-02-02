using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Web;
using BankLite.Api.Data;
using BankLite.Api.Dtos;
using BankLite.Api.Models;
using BankLite.Api.Services;

namespace BankLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BankLiteDbContext _db;
    private readonly PasswordService _pw;
    private readonly JwtService _jwt;

    public AuthController(BankLiteDbContext db, PasswordService pw, JwtService jwt)
    {
        _db = db;
        _pw = pw;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password required");

        // ✅ FIXED: Güçlü şifre politikası (min 8 karakter, uppercase, number)
        if (req.Password.Length < 8 || !req.Password.Any(char.IsUpper) || !req.Password.Any(char.IsDigit))
            return BadRequest("Password must be at least 8 chars with uppercase letter and number");

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (existing != null) return BadRequest("Username already taken");

        // ✅ FIXED: Role sadece "User" olarak atanıyor (Mass Assignment engellendi)
        var user = new User
        {
            Username = req.Username,
            FullName = req.FullName ?? req.Username,
            PasswordHash = _pw.Hash(req.Password),
            Role = "User",  // Kesinlikle sabit - hiçbir şekilde değiştirilmez
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.CreateToken(user);

        return Ok(new
        {
            token,
            username = user.Username,
            fullName = user.FullName,
            role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password required");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null) return Unauthorized("Invalid credentials");

        if (!_pw.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _jwt.CreateToken(user);

        return Ok(new
        {
            token,
            username = user.Username,
            fullName = user.FullName,
            role = user.Role
        });
    }

    // ✅ FIXED: Eski şifre kontrol edilir
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordRequest req)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound("User not found");

        // ✅ FIXED: Eski şifre doğrulanıyor
        if (string.IsNullOrWhiteSpace(req.OldPassword) || !_pw.Verify(req.OldPassword, user.PasswordHash))
            return Unauthorized("Current password is incorrect");

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest("New password must be at least 8 characters");

        if (!req.NewPassword.Any(char.IsUpper) || !req.NewPassword.Any(char.IsDigit))
            return BadRequest("Password must contain uppercase letter and number");

        user.PasswordHash = _pw.Hash(req.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }

    // ✅ REMOVED: Tüm kullanıcıları listeleme endpoint'i kaldırıldı (güvenlik açığı)

    // ✅ FIXED: IDOR prevention + Mass Assignment prevention
    [Authorize]
    [HttpPut("users/{userId}")]
    public async Task<ActionResult> UpdateUser(int userId, UpdateUserRequest req)
    {
        // ✅ FIXED: Token'daki kullanıcı ID'si kontrol ediliyor - sadece kendi profili düzenlenebilir
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized();

        if (currentUserId != userId)
            return Forbid("You can only update your own profile");

        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // ✅ FIXED: Role ASLA değiştirilemiyor (Mass Assignment engellendi)
        if (!string.IsNullOrWhiteSpace(req.Username))
        {
            var existingUsername = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.Id != userId);
            if (existingUsername != null) return BadRequest("Username already taken");
            user.Username = req.Username;
        }

        if (!string.IsNullOrWhiteSpace(req.FullName))
            user.FullName = req.FullName;

        // Küçük not: req.Role gönderilse bile yoksayılır

        await _db.SaveChangesAsync();

        return Ok(new { message = "User updated successfully" });
    }

    // ✅ REMOVED: SQL Injection güvenlik açığı olan endpoint kaldırıldı

    // ✅ REMOVED: Open Redirect güvenlik açığı olan endpoint kaldırıldı
}
