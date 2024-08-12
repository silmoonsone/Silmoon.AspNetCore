using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Binders;
using Silmoon.AspNetCore.Blazor;
using Silmoon.AspNetCore.Blazor.Extensions;
using Silmoon.AspNetCore.Extension.Binders;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Test;
using Silmoon.AspNetCore.Test.Components;
using Silmoon.AspNetCore.Test.Hubs;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.AspNetCore.Test.RazorPages;
using Silmoon.AspNetCore.Test.Services;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;

string ProjectName = Assembly.GetExecutingAssembly().GetName().Name;

Helper.RegisterStartClassSupport();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc(config =>
{
    config.ModelBinderProviders.Insert(0, new BigIntegerBinderProvider());
    config.ModelBinderProviders.Insert(0, new ObjectIdBinderProvider());
}).AddNewtonsoftJson();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "DataProtection"));
builder.Services.AddSession(o => { o.Cookie.Name = ProjectName + "_" + "Session"; });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
{
    o.LoginPath = new PathString("/signin");
    o.AccessDeniedPath = new PathString("/access_denied");
    o.Cookie.Name = ProjectName + "_" + "Cookie";
});


// ** required NuGet package for Swashbuckle.AspNetCore
//builder.Services.AddSwaggerGen(c => c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{ProjectName}.xml"), true)).AddSwaggerGenNewtonsoftSupport();

// ** required NuGet package for Swashbuckle.AspNetCore.Newtonsoft
//builder.Services.AddSwaggerGen().AddSwaggerGenNewtonsoftSupport();

// ** SignalR service mode for ChatServiceHub, require AddSignalR()
//builder.Services.AddSingleton<ChatService>();

// ** SignalR support
//builder.Services.AddSignalR();

// ** required NuGet package for Microsoft.AspNetCore.SignalR.Protocols.NewtonsoftJson
//builder.Services.AddSignalR().AddNewtonsoftJsonProtocol();

// ** To add Blazor service
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddSilmoonConfigure<SilmoonConfigureServiceImpl>(o =>
{
    o.ReleaseConfig();
});
builder.Services.AddSingleton<Core>();
builder.Services.AddSilmoonAuth<SilmoonAuthServiceImpl>();
builder.Services.AddJsComponentInterop();

//builder.Services.AddSilmoonDevApp<SilmoonDevAppServiceImpl>();

// ## custom a IHostedService for MainHostService
//builder.Services.AddSingleton<MainHostService>();
//builder.Services.AddHostedService(provider => provider.GetRequiredService<MainHostService>());

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

var app = builder.Build();

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c => c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None));
//}

//app.UseApiDecrypt();
app.UseCors(builder => builder.WithHosts("localhost"));

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area:exists?}/{controller=Home}/{action=Index}");

// ** To enable razor pages support
//app.MapRazorPages();

// ** To add a SignalR Hub
//app.MapHub<ChatHub>("/hubs/ChatHub");

// ** Use service mode for SignalR Hub
//app.MapHub<ChatServiceHub>("/hubs/ChatServiceHub");

// ** To enable Blazor server components
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.UseAntiforgery();

Helper.Output(logger, "Server app run.", LogLevel.Information);
app.Run();
Helper.Output(logger, "Server shutdown.", LogLevel.Information);