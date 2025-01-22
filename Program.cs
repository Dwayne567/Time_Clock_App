using FT_TTMS_WebApplication.Data; // Import the data context
using FT_TTMS_WebApplication.Interfaces; // Import the interfaces
using FT_TTMS_WebApplication.Models; // Import the models
using FT_TTMS_WebApplication.Repository; // Import the repositories
using FT_TTMS_WebApplication.Services; // Import the services
using Microsoft.AspNetCore.Authentication.Cookies; // Import cookie authentication
using Microsoft.AspNetCore.Identity; // Import identity services
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core

var builder = WebApplication.CreateBuilder(args); // Create a new WebApplication builder

// Add services to the container.
builder.Services.AddControllersWithViews(); // Add MVC controllers with views
builder.Services.AddScoped<IJobRepository, JobRepository>(); // Register JobRepository with the DI container
builder.Services.AddScoped<IDutyRepository, DutyRepository>(); // Register DutyRepository with the DI container
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Register UserRepository with the DI container

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the DbContext to use SQL Server with the connection string from configuration
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>() // Add Identity services
    .AddEntityFrameworkStores<ApplicationDbContext>() // Use Entity Framework stores
    .AddDefaultTokenProviders(); // Add default token providers

builder.Services.AddMemoryCache(); // Add memory caching
builder.Services.AddSession(); // Add session state

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Add cookie authentication
       .AddCookie(); // Configure cookie authentication

// Register EmailSender as IEmailSender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Configure EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build(); // Build the WebApplication

if (args.Length == 1 && args[0].ToLower() == "seeddata")
{
    // Seed the database with initial data if the "seeddata" argument is provided
    //Seed.SeedUsersAndRolesAsync(app);
    //Seed.SeedData(app);
    Seed.SeedJobsAsync(app);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Use the error handler for non-development environments
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); // Use HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files

app.UseRouting(); // Enable routing
app.UseSession(); // Enable session state
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Define the default route

app.Run(); // Run the application