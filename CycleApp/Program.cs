using CycleApp.DataAccess;
using CycleApp.Middleware;
using CycleApp.Services;
using CycleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddLogging(config => 
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>()
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<ICodeStorageService, CodeStorageService>()
                .AddScoped<ICycleCalculatorService, CycleCalculatorService>()
                .AddHostedService<CycleCalculationBackgroundService>();

// Add database context
builder.Services.AddDbContext<CycleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
string jwtKey = builder.Configuration["Jwt:Key"] ?? 
    throw new InvalidOperationException("JWT key is not configured");

var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                
                var authHeader = context.Request.Headers.Authorization.ToString();
                var tokenInfo = !string.IsNullOrEmpty(authHeader) && authHeader.Length > 15 ? 
                    $"Token starts with: {authHeader.Substring(7, 15)}..." : 
                    "No token provided";
                
                logger.LogWarning(
                    "Authentication failed JWT for {Path}: {ExceptionType} - {ExceptionMessage}. {TokenInfo}", 
                    context.Request.Path,
                    context.Exception.GetType().Name, 
                    context.Exception.Message,
                    tokenInfo);
                
                // Add additional details for token validation failures
                if (context.Exception is SecurityTokenValidationException)
                {
                    logger.LogWarning("Token validation details: Request method: {Method}, Query: {Query}, IP: {IP}", 
                        context.Request.Method,
                        context.Request.QueryString.Value,
                        context.Request.HttpContext.Connection.RemoteIpAddress);
                }
                
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // This is called when a user tries to access a secured endpoint but fails authentication
                if (context.Response.StatusCode == 401)
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning(
                        "JWT  Authorization failed 401: User attempted to access {Path} with method {Method}. " +
                        "Request IP: {IP}, User-Agent: {UserAgent}, Authorization Header Present: {AuthHeaderPresent}",
                        context.Request.Path,
                        context.Request.Method,
                        context.Request.HttpContext.Connection.RemoteIpAddress,
                        context.Request.Headers.UserAgent.ToString(),
                        !string.IsNullOrEmpty(context.Request.Headers.Authorization.ToString()));
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                var username = context.Principal?.Identity?.Name ?? "unknown";
                logger.LogInformation(
                    "JWT Token validated successfully for user {Username} at {Timestamp}", 
                    username, 
                    DateTime.UtcNow);
                return Task.CompletedTask;
            }
        };
    });

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
        {
            policyBuilder.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Token-Expired");
        }
        else
        {
            policyBuilder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// Build the application
var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure request pipeline
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CycleDbContext>();
    dbContext.Database.Migrate();
}

app.Run();