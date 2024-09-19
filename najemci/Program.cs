using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using najemci.Data;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

//public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//{
//    var defaultCulture = new CultureInfo("cs-CZ");
//    var localizationOptions = new RequestLocalizationOptions
//    {
//        DefaultRequestCulture = new RequestCulture(defaultCulture),
//        SupportedCultures = new List<CultureInfo> { defaultCulture },
//        SupportedUICultures = new List<CultureInfo> { defaultCulture }
//    };
//    app.UseRequestLocalization(localizationOptions);
//}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("najemciContext") ?? throw new InvalidOperationException("Connection string 'najemciContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
