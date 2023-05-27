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
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public Auth(ILogger<PostsController> logger, ApplicationContext db) {
        _logger = logger;
        _db = db;
        
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
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
        return FixedOk(user);
    }
    
    private ContentResult FixedOk<T>(T obj)
    {
        return Content(JsonSerializer.Serialize(obj, _jsonSerializerOptions), "application/json");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogOut() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    public class LoginBody {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}