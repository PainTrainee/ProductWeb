global using ProductWeb.Models;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.EntityFrameworkCore;
global using ProductWeb.Data;
using ProductWeb.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductContext>();

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ProductContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRoleService, RoleService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
