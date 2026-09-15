using System.Text;
using ERP.Application;
using ERP.Infrastructure;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// Enable Npgsql legacy timestamp behavior to ensure seamless compatibility with IST DateTime values
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add Controllers with JSON String Enum Converter
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Key"] ?? "ERP_Secure_JWT_Secret_Key_2026_Enterprise_System_Super_Secret_Key_!#9988";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ERPApplicationServer",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "ERPApplicationClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };
    options.Events = new JwtBearerEvents
    {
        OnForbidden = async context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\":\"Access Denied: You do not have permission for this resource.\"}");
            }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Employee"));
    options.AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "Employee", "Customer"));
});

// Dynamic Permission-Based Policy Provider, Authorization Handler & 403 Response Interceptor
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ERP.API.Security.PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, ERP.API.Security.PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ERP.API.Security.CustomAuthorizationMiddlewareResultHandler>();

// Configure Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise ERP Web API",
        Version = "v1",
        Description = "Enterprise Resource Planning Web API (.NET 10 + Clean Architecture + PostgreSQL Persistence + RBAC)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token directly (without 'Bearer ' prefix). Swagger UI automatically prepends 'Bearer '.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
    });
});

// Register Clean Architecture layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Ensure PostgreSQL database is initialized on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
    DbInitializer.Initialize(dbContext, builder.Configuration, app.Environment);
}

// Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Web API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// Configure Static File Hosting for Frontend (CompanyDashboard)
var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "../../Frontend/CompanyDashboard"));
if (!Directory.Exists(frontendPath))
{
    frontendPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Frontend/CompanyDashboard"));
}

if (Directory.Exists(frontendPath))
{
    var fileProvider = new PhysicalFileProvider(frontendPath);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback to SPA index.html for non-API client routes
app.MapFallback(async context =>
{
    // Unmatched API routes return 404 JSON instead of HTML
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"API endpoint not found.\"}");
        return;
    }

    if (Directory.Exists(frontendPath))
    {
        var indexPath = Path.Combine(frontendPath, "index.html");
        if (File.Exists(indexPath))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(indexPath);
            return;
        }
    }

    context.Response.StatusCode = StatusCodes.Status404NotFound;
});

app.Run();
