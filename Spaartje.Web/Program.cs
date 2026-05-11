using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spaartje.Web.Data;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure cookie settings for authentication
 builder.Services.ConfigureApplicationCookie(options => {
     options.LoginPath = "/Login";
     options.LogoutPath = "/Logout";
     options.AccessDeniedPath = "/Login";
     // Set cookie expiration to 7 days
     options.ExpireTimeSpan = TimeSpan.FromDays(7);
     // Enable sliding expiration to refresh the cookie on each request
     options.SlidingExpiration = true;
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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();


app.Run();
