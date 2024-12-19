using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json.Serialization;
using TradingApp.AutoMapper;
using TradingApp.Data;
using TradingApp.Filter;
using TradingApp.MIddlewears;
using TradingApp.Models;
using TradingApp.Models.Config;
using TradingApp.Repositories;

var builder = WebApplication.CreateBuilder(args);


// Register AutoMapper with the mapping profiles
builder.Services.AddAutoMapper(typeof(MappingProfile));  // Register AutoMapper with your profile

builder.Services.AddScoped<InvoiceRepository>();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//only for identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// for other than identity
builder.Services.AddDbContext<TradingAppContext>((serviceProvider, options) =>
    options.UseSqlServer(connectionString)
            .EnableSensitiveDataLogging()
            .LogTo(LogQueryToConsole, LogLevel.Information)
);

void LogQueryToConsole(string log)
{
    if (log.Contains("Executed DbCommand"))
    {
        Console.WriteLine(ConvertToExecutableQuery(log));
    }
}

string ConvertToExecutableQuery(string log)
{
    // Extract the SQL command and parameters from the log
    var sqlCommandIndex = log.IndexOf("INSERT INTO");
    if (sqlCommandIndex == -1) return log;

    var sqlCommand = log.Substring(sqlCommandIndex);

    // Extract parameters from the log
    var parametersIndex = log.IndexOf("Parameters=[");
    if (parametersIndex != -1)
    {
        var parametersText = log.Substring(parametersIndex);
        var parameters = parametersText
            .Split('[', ']')
            .Where((_, i) => i % 2 != 0) // Extracting parameter pairs
            .Select(param => param.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());

        foreach (var param in parameters)
        {
            var placeholder = param.Key;
            var value = param.Value;

            // Determine if the value is NULL
            if (value.ToLower().Contains("null"))
            {
                sqlCommand = sqlCommand.Replace(placeholder, "NULL");
            }
            else
            {
                // Determine if it's a string or a number type (assumes string values are enclosed in quotes)
                if (value.StartsWith("'") && value.EndsWith("'"))
                {
                    sqlCommand = sqlCommand.Replace(placeholder, value);
                }
                else
                {
                    sqlCommand = sqlCommand.Replace(placeholder, value);
                }
            }
        }
    }

    return sqlCommand;
}
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

//commit durind development
//app.UseMiddleware<ExceptionHandlingMiddleware>();

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