using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json.Serialization;
using TradingApp.AutoMapper;
using TradingApp.Data;
using TradingApp.Filter;
using TradingApp.Models;
using TradingApp.Models.Config;

var builder = WebApplication.CreateBuilder(args);


// Register AutoMapper with the mapping profiles
builder.Services.AddAutoMapper(typeof(MappingProfile));  // Register AutoMapper with your profile



// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//only for identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// for other than identity
builder.Services.AddDbContext<TradingAppContext>((serviceProvider, options) =>
    options.UseSqlServer(connectionString));
   
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser> (options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new BinderProvider());

}).AddViewOptions(options =>
{
   
    options.HtmlHelperOptions.ClientValidationEnabled = true;
}
).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;

});
// Configure the default display format for DateTime properties
builder.Services.Configure<MvcOptions>(options =>
{
    options.ModelMetadataDetailsProviders.Add(new DateTimeFormatMetadataProvider());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

// Add a more specific route for Invoice
app.MapControllerRoute(
    name: "invoice",
    pattern: "Invoice/{action=Index}/{id?}",
    defaults: new { controller = "TradingDocuments" });

app.MapControllerRoute(
    name: "Customer",
    pattern: "Customer/{action=Index}/{id?}",
    defaults: new { controller = "Stakeholders" });

app.MapRazorPages();


app.Run();
public class DateTimeFormatMetadataProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context?.Key.MetadataKind == ModelMetadataKind.Property &&
            context?.Key.ModelType == typeof(DateTime))
        {
            context.DisplayMetadata.NullDisplayText = "";
            context.DisplayMetadata.DisplayFormatString = "{0:d}";
        }
    }
}