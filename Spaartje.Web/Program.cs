// WEB/Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB.Data;
using WEB.Models;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ──────────────────────────────────────────────────────────────
// WHY AddDbContext here and not in a separate file?
// For the monolithic start, Program.cs is the composition root —
// the single place where all dependencies are wired together.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure() // Handles transient SQL failures
    )
);

// ── 2. IDENTITY ──────────────────────────────────────────────────────────────
// WHY AddIdentity vs AddDefaultIdentity?
// AddDefaultIdentity only registers a basic user. AddIdentity registers
// BOTH UserManager and RoleManager, which you need for role-based auth.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy — adjust to your security requirements
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Lockout settings — protects against brute force attacks
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()  // Tells Identity to use EF Core
.AddDefaultTokenProviders();                        // Needed for password reset tokens

// ── 3. COOKIE CONFIGURATION ──────────────────────────────────────────────────
// WHY configure cookies explicitly?
// The defaults redirect to /Account/Login which doesn't match Razor Pages
// conventions. These paths must match your actual page routes.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true; // Resets expiry on activity
    options.Cookie.HttpOnly = true;   // Prevents JavaScript access (XSS protection)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only in prod
});

// ── 4. AUTHORIZATION POLICIES ────────────────────────────────────────────────
// WHY named policies instead of just [Authorize(Roles = "Admin")]?
// Policies are more flexible — you can add claims, custom requirements,
// or multiple roles later without changing your page attributes.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireUser", policy =>
        policy.RequireRole("Admin", "User")); // Admin can do everything User can

    // Future example with claims:
    // options.AddPolicy("CanViewReports", policy =>
    //     policy.RequireClaim("permission", "reports.view"));
});

// ── 5. RAZOR PAGES ───────────────────────────────────────────────────────────
builder.Services.AddRazorPages(options =>
{
    // WHY AuthorizeFolder instead of individual page attributes?
    // It's DRY — one line protects every page in the folder.
    // New pages added to /Admin are automatically protected.
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Dashboard", "RequireUser");

    // Public pages (no attribute needed — they're public by default)
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
   
});

var app = builder.Build();

// ── 6. SEED DATA ─────────────────────────────────────────────────────────────
// WHY create a scope here?
// Seeding requires scoped services (DbContext, UserManager). The app's
// root container is the singleton scope — you must create a child scope
// to resolve scoped services safely during startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Apply pending migrations automatically
    // WHY MigrateAsync instead of EnsureCreated?
    // EnsureCreated skips migrations entirely — fine for prototypes,
    // but breaks in production. MigrateAsync applies any unapplied migrations.
    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await SeedData.SeedRolesAndAdminAsync(services);
}

// ── 7. MIDDLEWARE PIPELINE ───────────────────────────────────────────────────
// WHY does ORDER matter here?
// ASP.NET Core middleware runs in the exact order you register it.
// Authentication must run before Authorization — you can't check
// what a user is allowed to do before you know who they are.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // ← Who are you?
app.UseAuthorization();  // ← What are you allowed to do?

app.MapRazorPages();

app.Run();