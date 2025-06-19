using ExpenseTrackerApi.Data;
using ExpenseTrackerApi.DTOs;
using ExpenseTrackerApi.Helper;
using ExpenseTrackerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseTrackerApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("email already in use");
            }
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = Hash(request.Password)
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return Ok("registration successful!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("UserName/Password should not be empty");
            }

            var hash = Hash(request.Password);
            var user = await _db.Users.FirstOrDefaultAsync(u => string.Equals(u.Email, request.Email) && string.Equals(u.PasswordHash, hash));
            if (user == null)
            {
                return BadRequest("Invalid Credentials");
            }

            var token = JwtTokenHelper.GenerateToken(user, _config["JwtSettings:Key"]!);

            return Ok(new { Token = token, Name = user.FullName, Email = user.Email });

        }
        private static string Hash(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
