using Core.Config;
using Core.Data;
using Core.Models;
using Logic.Helpers;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDropdownHelper, DropdownHelper>();


builder.Services.AddScoped<IAdminHelper, AdminHelper>();

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("eFashionTB"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 5;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IEmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
builder.Services.AddSingleton<IGeneralConfiguration>(builder.Configuration.GetSection("GeneralConfiguration").Get<GeneralConfiguration>());
builder.Services.AddTransient<IEmailService, EmailService>();

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
