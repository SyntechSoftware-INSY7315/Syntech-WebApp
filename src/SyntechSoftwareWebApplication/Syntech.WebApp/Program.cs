using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Services;
using Syntech.WebApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

builder.Services.AddScoped<IChatbotService, ChatbotService>();

//
// ============================================================================
// SERVICE CONFIGURATION
// ============================================================================
//

// -----------------------------------------------------------------------------
// MVC
// -----------------------------------------------------------------------------
// Registers ASP.NET Core MVC services.
//
// This allows the application to use:
// - Controllers
// - Razor Views
// - Model binding
// - Validation
// - Dependency injection
//
builder.Services.AddControllersWithViews();


// -----------------------------------------------------------------------------
// Cookie Authentication
// -----------------------------------------------------------------------------
// Configures ASP.NET Core's cookie-based authentication system.
//
// We are using cookies rather than ASP.NET Core Identity's default database
// user store because the project already has a custom User table.
//
// Once a user successfully logs in, ASP.NET Core creates an encrypted
// authentication cookie in the user's browser.
//
// That cookie is automatically sent with subsequent requests, allowing
// ASP.NET Core to identify the authenticated user.
//
// The cookie does NOT contain the user's password.
// -----------------------------------------------------------------------------
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // ---------------------------------------------------------------------
        // LoginPath
        // ---------------------------------------------------------------------
        // When an unauthenticated user attempts to access a protected page,
        // ASP.NET Core redirects them to this URL.
        //
        // We will create this page in a later step.
        //
        options.LoginPath = "/Account/Login";

        // ---------------------------------------------------------------------
        // AccessDeniedPath
        // ---------------------------------------------------------------------
        // If a user is authenticated but does not have permission to access
        // a protected resource, they will be redirected here.
        //
        // Example:
        // A Client attempts to access an Admin-only page.
        //
        options.AccessDeniedPath = "/Account/AccessDenied";

        // ---------------------------------------------------------------------
        // Cookie name
        // ---------------------------------------------------------------------
        // Gives the authentication cookie an explicit application-specific
        // name.
        //
        // The browser will store this cookie after successful authentication.
        //
        options.Cookie.Name = "Syntech.Authentication";

        // ---------------------------------------------------------------------
        // HttpOnly
        // ---------------------------------------------------------------------
        // Prevents client-side JavaScript from directly reading the cookie.
        //
        // This reduces the risk of cookie theft through certain types of
        // client-side attacks.
        //
        options.Cookie.HttpOnly = true;

        // ---------------------------------------------------------------------
        // SecurePolicy
        // ---------------------------------------------------------------------
        // Ensures the authentication cookie is only transmitted over HTTPS.
        //
        // Our ASP.NET Core development application uses HTTPS, and production
        // should also use HTTPS.
        //
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        // ---------------------------------------------------------------------
        // SameSite
        // ---------------------------------------------------------------------
        // Helps protect against certain cross-site request forgery scenarios.
        //
        // Lax is appropriate for a normal MVC web application's authentication
        // cookie.
        //
        options.Cookie.SameSite =
            SameSiteMode.Lax;

        // ---------------------------------------------------------------------
        // Session lifetime
        // ---------------------------------------------------------------------
        // Determines how long the authentication session remains valid.
        //
        // We will use a sliding expiration so that active users can continue
        // working without being unexpectedly logged out.
        //
        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(60);

        options.SlidingExpiration = true;
    });


// -----------------------------------------------------------------------------
// Entity Framework Core
// -----------------------------------------------------------------------------
// Registers our ApplicationDbContext with ASP.NET Core's dependency
// injection container.
//
// ApplicationDbContext is responsible for communicating with the existing
// SQL Server database.
//
// The connection string is stored in ASP.NET Core User Secrets during
// development so that the database password is not committed to GitHub.
//
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");


// Fail immediately with a clear error if the connection string has not
// been configured.
//
// This is preferable to allowing the application to start and producing
// a confusing database-related error later.
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The database connection string 'DefaultConnection' was not found. " +
        "Configure it using ASP.NET Core User Secrets."
    );
}


// Register ApplicationDbContext.
//
// SQL Server is the database provider used by the team's shared database.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// -----------------------------------------------------------------------------
// Authentication service
// -----------------------------------------------------------------------------
// Registers our custom authentication business-logic service.
//
// Scoped lifetime means one AuthenticationService instance is created for
// each HTTP request. This is appropriate because the service depends on
// ApplicationDbContext, which is also registered with a scoped lifetime.
//
builder.Services.AddScoped<
    IAuthenticationService,
    AuthenticationService>();

//
// ============================================================================
// APPLICATION BUILD
// ============================================================================
//

var app = builder.Build();

// ============================================================================
// INITIAL ADMINISTRATOR SEEDING
// ============================================================================
//
// Creates the initial administrator account when the database does not
// already contain an Admin record.
//
// The credentials are read from ASP.NET Core User Secrets rather than
// being hard-coded into source code.
//
// The seeder is idempotent, so restarting the application will not create
// duplicate administrator accounts.
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<ApplicationDbContext>();

    await AdminSeeder.SeedAsync(
        context,
        builder.Configuration);
}

//
// ============================================================================
// HTTP REQUEST PIPELINE
// ============================================================================
//

// -----------------------------------------------------------------------------
// Error handling
// -----------------------------------------------------------------------------
// In development, detailed exceptions are useful for debugging.
//
// In production, detailed exception information should not be exposed to
// users because it can reveal sensitive application information.
//
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // The default HSTS period is 30 days.
    // HSTS instructs browsers to use HTTPS for the application.
    app.UseHsts();
}


// -----------------------------------------------------------------------------
// HTTPS redirection
// -----------------------------------------------------------------------------
// Redirects HTTP requests to HTTPS.
//
app.UseHttpsRedirection();


// -----------------------------------------------------------------------------
// Static files
// -----------------------------------------------------------------------------
// Allows the application to serve files from wwwroot.
//
// Examples:
// - CSS
// - JavaScript
// - Images
//
app.UseStaticFiles();


app.UseRouting();


// -----------------------------------------------------------------------------
// Authentication
// -----------------------------------------------------------------------------
// Reads the authentication cookie from incoming requests and determines
// whether the current user is authenticated.
//
// This MUST run before authorization.
// -----------------------------------------------------------------------------
app.UseAuthentication();


// -----------------------------------------------------------------------------
// Authorization
// -----------------------------------------------------------------------------
// Determines whether the authenticated user has permission to access the
// requested resource.
//
// For example:
//
// [Authorize]
// [Authorize(Roles = "Admin")]
//
// -----------------------------------------------------------------------------
app.UseAuthorization();


// -----------------------------------------------------------------------------
// Default MVC route
// -----------------------------------------------------------------------------
// Maps URLs using the conventional MVC pattern:
//
// /Controller/Action/Id
//
// Example:
// /Home/Index
//
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


// -----------------------------------------------------------------------------
// Start the application
// -----------------------------------------------------------------------------
app.Run();