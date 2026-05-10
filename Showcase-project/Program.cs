using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Showcase_API.Data;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthConnection")));
builder.Services.AddAuthorization();

// only add Identity if not already registered
if (!builder.Services.Any(sd => sd.ServiceType == typeof(UserManager<IdentityUser>)))
{
    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // optional: tune password / lockout rules here
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();
}

// only add identity API endpoints if Identity wasn't registered previously
if (!builder.Services.Any(sd => sd.ServiceType == typeof(UserManager<IdentityUser>)))
{
    builder.Services.AddIdentityApiEndpoints<IdentityUser>();
}

builder.Services.AddAuthentication();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");

app.MapControllers();

var authGroup = app.MapGroup("/api/auth");

// only map identity API endpoints if Identity services exist (prevents duplicate setup during mapping)
if (app.Services.GetService<UserManager<IdentityUser>>() is not null)
{
    authGroup.MapIdentityApi<IdentityUser>();
}

// DB seeding and identity role/user seeding
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {

    }
    catch (Exception)
    {

        throw;
    }
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // create admin user (change values for production)
    var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@local";
    var adminPassword = builder.Configuration["AdminUser:Password"] ?? "Admin123!";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();