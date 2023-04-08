using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Netial.Data;
using Netial.Database;
using Netial.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddDbContext<ApplicationContext>();
builder.Services.AddOptions();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");

var app = builder.Build();

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

app.MapGet("/testuser", (ApplicationContext db) => {
    return db.Users.ToList();
});
app.MapGet("/hash/{passwd}/{salt}", (HttpRequest request, [FromRoute] string passwd, [FromRoute] string salt) => {
    if (string.IsNullOrEmpty(passwd) || string.IsNullOrEmpty(salt)) {
        return "400";
    }
    return Netial.Helpers.Cryptography.Sha256Hash(passwd+salt); 
});
app.MapPost("/login", async (string? returnUrl, HttpContext context, ApplicationContext db) => {

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
    var query = db.Users
        .AsEnumerable()
        .Where(u => u.EmailNormalized == emailNormalized
                    && u.PasswordHash == Cryptography.Sha256Hash(password+u.PasswordSalt));

    if (!query.Any()) {
        return Results.Redirect("/login?invalid");
    }
    
    var claims = new List<Claim> {
        new Claim(ClaimTypes.Name, query.ElementAt(0).FirstName),
        new Claim(ClaimTypes.Email, query.ElementAt(0).Email)
    };
    // создаем объект ClaimsIdentity
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    // установка аутентификационных куки
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    return Results.Redirect(returnUrl ?? "/");
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
