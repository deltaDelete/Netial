using System.Globalization;
using System.Security.Claims;
using System.Text;
using Ljbc1994.Blazor.IntersectionObserver;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Netial.Api;
using Netial.Database;
using Netial.Database.Models;
using Netial.Helpers;
using Netial.Services;

internal static class Program {
    private static readonly string[] REGISTER_FIELDS =
        { "lastname", "firstname", "birthdate", "email", "password", "password2" };

    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.ConfigureServices();

        var app = builder.Build();

        app.ConfigureApplication();

        app.ConfigureMinimalApi();

        app.Run();
    }

    private static void ConfigureMinimalApi(this WebApplication app) {
        app.MapGet("/testuser", (ApplicationContext db) => { return db.Users.ToList(); });
        app.MapGet("/security/hash", GenHash);
        app.MapPost("/account/login", Login);
        app.MapPost("/account/logout", Logout);
        app.MapPost("/account/register", Register);
        app.MapGet("/images/users/{id}", GetUserImage);
        app.MapGet("/images/attachments/{id}", GetAttachmentImage);
    }

    private static void ConfigureApplication(this WebApplication app) {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.MapControllers();

        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }

    private static void ConfigureServices(this IServiceCollection services) {
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddSingleton<EmailService>();
        services.AddDbContextFactory<ApplicationContext>(options => options.UseLazyLoadingProxies());
        services.AddOptions();
        services.AddMudServices();
        services.AddIntersectionObserver();

        services.AddAuthorization();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => options.LoginPath = "/login");

        services.AddHttpClient();
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    private static async Task<IResult> Register(string? returnUrl, HttpContext context, ApplicationContext db) {
        var form = context.Request.Form;

        List<string> missingKeys = new List<string>();
        List<string> emptyKeys = new List<string>();
        foreach (string s in REGISTER_FIELDS) {
            if (!form.ContainsKey(s)) {
                missingKeys.Add(s);
                break;
            }

            if (string.IsNullOrEmpty(form[s])) {
                emptyKeys.Add(s);
            }
        }

        if (missingKeys.Any()) {
            return Results.BadRequest(missingKeys);
        }

        if (emptyKeys.Any()) {
            return Results.BadRequest(missingKeys);
        }

        string lastname = form["lastname"];
        string firstname = form["firstname"];
        DateTime birthDate = DateTime.Parse(form["birthdate"]);
        string email = form["email"];
        string emailNormalized = email.ToUpper();
        string password = form["password"];
        string password2 = form["password2"];
        if (password != password2) {
            return Results.Redirect($"/account/register?invalid={Uri.EscapeDataString("Пароли не совпадают")}");
        }

        if (db.Users.Any(u => u.EmailNormalized == emailNormalized)) {
            return Results.Redirect("/account/register?invalid");
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

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var returnRoute = returnUrl is null ? "/account/login" : $"/account/login?returnUrl={returnUrl}";
        return Results.Redirect(returnRoute);
    }

    private static async Task<IResult> Login(string? returnUrl, HttpContext context, ApplicationContext db) {
        var form = context.Request.Form;
        if (!form.ContainsKey("email") || !form.ContainsKey("password")) {
            return Results.Redirect("/login?invalid");
        }

        string email = form["email"];
        string password = form["password"];

        if (string.IsNullOrWhiteSpace(password)) {
            return Results.Redirect("/login?invalid");
        }

        var emailNormalized = email.ToUpper();
        var query = db.Users.AsEnumerable()
            .Where(u => u.EmailNormalized == emailNormalized &&
                        u.PasswordHash == Cryptography.Sha256Hash(password + u.PasswordSalt));

        if (!query.Any()) {
            return Results.Redirect("/account/login?invalid");
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
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return Results.Redirect(returnUrl ?? "/");
    }

    private static async Task<IResult> Logout(HttpContext context) {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.LocalRedirect("/");
    }

    private static IResult GenHash(string passwd, string salt) {
        if (string.IsNullOrEmpty(passwd) || string.IsNullOrEmpty(salt)) {
            return Results.BadRequest("Отсутствует параметр");
        }

        return Results.Text(Cryptography.Sha256Hash(passwd + salt));
    }

    private static IResult GetUserImage(string id, IWebHostEnvironment env) {
        var fileinfo = env.WebRootFileProvider.GetFileInfo($"images/users/{id}.jpg");
        if (!fileinfo.Exists) {
            return Results.File(env.WebRootFileProvider.GetFileInfo("images/users/blank.png").CreateReadStream());
        }

        return Results.File(fileinfo.CreateReadStream());
    }

    private static async Task<IResult> GetAttachmentImage(string id, IWebHostEnvironment env) {
        var fileinfo = env.WebRootFileProvider.GetFileInfo($"images/attachments/{id}.jpg");
        if (!fileinfo.Exists) {
            return Results.File(env.WebRootFileProvider.GetFileInfo("images/unavailable.png").CreateReadStream());
        }

        return Results.File(fileinfo.CreateReadStream(), fileDownloadName: fileinfo.Name);
    }
}