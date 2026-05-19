using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spaartje.BLL.Services;
using Spaartje.DAL.Data;
using Spaartje.DAL.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Register the UserRepository and UserService with the dependency injection container.
// This allows us to inject IUserService into our Razor Pages and have it automatically
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
options.Password.RequireDigit = false;
options.Password.RequireLowercase = false;  
options.Password.RequireNonAlphanumeric = false;
options.Password.RequireUppercase = false;
options.Password.RequiredLength = 6;
options.SignIn.RequireConfirmedAccount = false;
 })
 .AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
// Add default token providers for password reset, email confirmation, etc.
.AddDefaultTokenProviders();


// Configure cookie settings for authentication
 builder.Services.ConfigureApplicationCookie(options => {
    // Set the paths for login, logout, and access denied actions
     options.LoginPath = "/Login";
     options.LogoutPath = "/Logout";
     options.AccessDeniedPath = "/AccessDenied";
     // Set cookie expiration to 7 days
     options.ExpireTimeSpan = TimeSpan.FromDays(7);
     // Enable sliding expiration to refresh the cookie on each request
     options.SlidingExpiration = true;
 });

// When someone asks for IUserRepository, give them a UserRepository.
builder.Services.AddScoped<IUserRepository, UserRepository>();

// When someone asks for IUserService, give them a UserService.
builder.Services.AddScoped<IUserService, UserService>();

// Register Category and Transaction repositories and services.
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// This tells ASP.NET Core: When a constructor asks for ICategoryService, create a CategoryService and give it to them.
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Register Transaction repository and service.
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// This tells ASP.NET Core: When a constructor asks for ITransactionService, create a TransactionService and give it to them.
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Register DashboardService and its interface.
builder.Services.AddScoped<IDashboardService, DashboardService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

  await DbSeeder.SeedAsync(services);

}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();


app.Run();
