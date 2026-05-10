using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Showcase_API.Data;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthConnection")));
builder.Services.AddAuthorization();


// inside an extension: only add Identity if not already registered
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

        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();
}


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

// Register endpoint: create user and return assigned roles
authGroup.MapPost("register", async (
    UserManager<IdentityUser> userManager,
    ILogger<Program> logger,
    RegisterRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Email and Password required" });

    var existing = await userManager.FindByEmailAsync(req.Email);
    if (existing != null)
        return Results.Conflict(new { error = "User already exists" });

    var user = new IdentityUser { UserName = req.Email, Email = req.Email, EmailConfirmed = true };
    var createResult = await userManager.CreateAsync(user, req.Password);
    if (!createResult.Succeeded)
    {
        return Results.BadRequest(createResult.Errors.Select(e => e.Description));
    }

    // Optionally add default roles here (keep for admin creation elsewhere)
    // e.g., await userManager.AddToRoleAsync(user, "User");

    var roles = await userManager.GetRolesAsync(user);

    logger.LogInformation("User {Email} created.", req.Email);
    return Results.Created($"/api/auth/users/{user.Id}", new { user.Id, user.Email, roles });
});

// Login endpoint: sign in and return user roles + admin flag
authGroup.MapPost("login", async (
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Email and Password required" });

    var user = await userManager.FindByEmailAsync(req.Email);
    if (user == null)
        return Results.Unauthorized();

    var result = await signInManager.PasswordSignInAsync(req.Email, req.Password, isPersistent: false, lockoutOnFailure: false);
    if (!result.Succeeded)
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var isAdmin = roles.Contains("Admin");

    // For real apps: return a JWT or auth cookie; here return role info for debugging/visibility
    return Results.Ok(new { message = "Login successful", email = user.Email, roles, isAdmin });
});

// Me endpoint: returns current signed-in user's roles and admin status
authGroup.MapGet("me", async (
    HttpContext http,
    UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.GetUserAsync(http.User);
    if (user == null)
        return Results.Unauthorized();

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new { email = user.Email, roles, isAdmin = roles.Contains("Admin") });
}).RequireAuthorization();

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
    else
    {
        // Ensure admin user is in Admin role
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();

internal record RegisterRequest(string Email, string Password);
internal record LoginRequest(string Email, string Password);
