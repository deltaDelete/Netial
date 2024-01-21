using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Netial.Database;
using Netial.Database.Models;
using Netial.Helpers;

namespace Netial.Api; 

[Route("api/[controller]")]
[ApiController]
public class Auth : ControllerBase {
    private readonly ILogger<PostsController> _logger;
    private readonly ApplicationContext _db;

    public Auth(ILogger<PostsController> logger, ApplicationContext db) {
        _logger = logger;
        _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginBody credentials) {
        string email = credentials.Email;
        string password = credentials.Password;

        if (string.IsNullOrWhiteSpace(password)) {
            return BadRequest(@"{""error"": ""password can't be null or whitespace""}");
        }

        var emailNormalized = email.ToUpper();
        var query = _db.Users.AsEnumerable()
            .Where(u => u.EmailNormalized == emailNormalized &&
                        u.PasswordHash == Cryptography.Sha256Hash(password + u.PasswordSalt));

        if (!query.Any()) {
            return NotFound(@"{""error"": ""no user found""}");
        }

        var user = query.ElementAt(0);

        // заполняем Claim'ы чтобы по ним получать информацию о текущем пользователе
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("BirthDate", user.BirthDate.ToString(DateTimeFormatInfo.InvariantInfo)),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        // создаем объект ClaimsIdentity
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        // установка аутентификационных куки
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return this.FixedOk(user);
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> LogOut() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterBody body) {
        string lastname = body.LastName;
        string firstname = body.FirstName;
        DateTime birthDate = body.BirthDate;
        string email = body.Email;
        string emailNormalized = email.ToUpper();
        string password = body.Password;
        string password2 = body.PasswordConfirm;
        if (password != password2) {
            return BadRequest("{\"error\": \"passwords do not match\"}");
        }

        if (_db.Users.Any(u => u.EmailNormalized == emailNormalized)) {
            return Conflict("{\"error\": \"user with provided email already exists\"}");
        }

        string passwordSalt = Guid.NewGuid().ToString().Split('-')[0];
        string passwordHash = Cryptography.Sha256Hash(password + passwordSalt);

        var user = new User() {
            Email = email,
            EmailNormalized = emailNormalized,
            FirstName = firstname,
            LastName = lastname,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Rating = 0,
            BirthDate = birthDate
        };

        var addedUser = await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        return this.FixedOk(addedUser.Entity);
    }

    public class LoginBody {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterBody {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
        [MinLength(8)]
        public string PasswordConfirm { get; set; } = string.Empty;
    }
}