using Microsoft.AspNetCore.Authentication.Cookies;
using Silmoon.AspNetCore.Encryption.Extensions;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.UserAuthTest;
using Silmoon.AspNetCore.UserAuthTest.Services;
using System.Reflection;


string ProjectName = Assembly.GetExecutingAssembly().GetName().Name;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddSession(o => { o.Cookie.Name = ProjectName + "_" + "Session"; });
builder.Services.AddSilmoonAuth<SilmoonAuthServiceImpl>();
builder.Services.AddWebAuthn<WebAuthnServiceImpl>();
builder.Services.AddSingleton<Core>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
{
    o.LoginPath = new PathString("/signin");
    o.AccessDeniedPath = new PathString("/access_denied");
    o.Cookie.Name = ProjectName + "_" + "Cookie";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();
app.UseWebAuthn();
app.UseSilmoonAuth();

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

app.Run();
