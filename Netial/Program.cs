using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Netial.Data;
using Netial.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddDbContext<ApplicationContext>();
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

app.UseAuthorization();
app.MapGet("/testuser", (ApplicationContext db) => {
    return db.Accounts.ToList();
});
app.MapGet("/hash/{passwd}/{salt}", (HttpRequest request, [FromRoute] string passwd, [FromRoute] string salt) => {
    if (string.IsNullOrEmpty(passwd) || string.IsNullOrEmpty(salt)) {
        return "400";
    }
    return Netial.Helpers.Cryptography.Sha256Hash(passwd+salt); 
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
