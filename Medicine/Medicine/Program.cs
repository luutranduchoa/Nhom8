using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Medicine.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Medicine.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for production stability
builder.WebHost.ConfigureKestrel(options =>
{
    // Maximum request body size (100 MB for file uploads)
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;

    // Connection timeout (2 minutes)
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);

    // Request timeout (30 seconds)
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);

    // HTTP/2 support
    options.ListenAnyIP(5054, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

// Add services to the container.

// Configure controllers and JSON options (use camelCase and avoid reference cycle issues)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Allow frontend apps running on common dev ports to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Allow common dev frontend origins. Add your frontend port(s) here.
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5175",
                "https://localhost:5175"
            )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // allow sending credentials if frontend needs them
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add response compression for larger responses
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json", "text/plain" });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicine API", Version = "v1" });

    // Cấu hình JWT cho Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng scheme Bearer. Ví dụ: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<MedicalDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MedicalDbConnection"),
        sqlServerOptions =>
        {
            // Connection pooling settings with retry on failure
            sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3);

            sqlServerOptions.CommandTimeout(30); // 30 seconds command timeout
        });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Relax password rules so simple password like "hoa123" can be used for local/dev seeding
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<MedicalDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MedicineApi",
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:7149",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "superSecretKey@3456789012345678901234567890"))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable developer exception page so swagger.json errors are visible during development
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling for all unhandled errors
app.UseGlobalExceptionHandler();

// Enable response compression
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS for frontend
app.UseCors("AllowFrontend");

// Add request logging middleware for development diagnostics
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var startTime = DateTime.UtcNow;
        var request = context.Request;

        // Log request info
        Console.WriteLine($"[{startTime:HH:mm:ss.fff}] {request.Method} {request.Path} {request.QueryString}");

        if (request.ContentType?.Contains("application/json") == true && request.Body.CanSeek)
        {
            request.Body.Position = 0;
            using (var reader = new System.IO.StreamReader(request.Body))
            {
                var body = await reader.ReadToEndAsync();
                if (!string.IsNullOrEmpty(body) && body.Length < 500)
                {
                    Console.WriteLine($"  Body: {body}");
                }
                request.Body.Position = 0;
            }
        }

        await next.Invoke();

        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"  Response: {context.Response.StatusCode} ({duration.TotalMilliseconds}ms)");
    });
}

// Removed duplicate/undefined CORS policy usage. Keep only the configured policy "AllowFrontend".

// 1. CHÈN ĐOẠN NÀY VÀO: Cấu hình bắt buộc để đọc file ảnh
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed default admin user/role for local development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var adminEmail = "luutranduchoa@gmail.com";
        var adminPassword = "hoa123"; // NOTE: weak password allowed for local/dev only
        var adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Admin",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRole);
                Console.WriteLine($"Seed: created admin {adminEmail}");
            }
            else
            {
                Console.WriteLine($"Seed: failed to create admin {adminEmail}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            if (!await userManager.CheckPasswordAsync(admin, adminPassword))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                var resetResult = await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);

                if (resetResult.Succeeded)
                {
                    Console.WriteLine($"Seed: reset password for existing admin {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Seed: failed to reset password for existing admin {adminEmail}: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                await userManager.AddToRoleAsync(admin, adminRole);
                Console.WriteLine($"Seed: added role {adminRole} to existing user {adminEmail}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed error: {ex.Message}");
    }
}

// Try to run the app. If the configured address/port is already in use,
// pick a free ephemeral port and run on that instead to avoid the "address
// already in use" crash during local debugging.
int GetFreePort()
{
    var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

try
{
    app.Run();
}
catch (System.IO.IOException ex) when (ex.Message?.Contains("address already in use") == true || ex.InnerException is System.Net.Sockets.SocketException)
{
    var port = GetFreePort();
    var url = $"http://127.0.0.1:{port}";
    Console.WriteLine($"Port conflict detected, switching to {url}");
    // Override the URLs the app listens on and try again
    app.Urls.Clear();
    app.Urls.Add(url);
    app.Run();
}
