//using CycleApp.DataAccess;
//using CycleApp.Models;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


//builder.Services.AddDbContext<CycleDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

//var app = builder.Build();


//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<CycleDbContext>();


//    dbContext.Database.EnsureDeleted(); 


//    dbContext.Database.EnsureCreated();
//}

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();




//using CycleApp.DataAccess;
//using CycleApp.Middleware;
//using CycleApp.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using CycleApp.Services.Interfaces;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<CycleDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<ICodeStorageService, CodeStorageService>();
//builder.Services.AddScoped<ICycleCalculatorService, CycleCalculatorService>();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
//    {
//        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
//        if (origins != null && origins.Length > 0)
//        {
//            policyBuilder.WithOrigins(origins)
//                          .AllowAnyHeader()
//                          .AllowAnyMethod();
//        }
//    });
//});



//var app = builder.Build();
//app.UseCors("AllowSpecificOrigins");

//app.UseMiddleware<ErrorHandlingMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseCors("AllowSpecificOrigins");
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<CycleDbContext>();
//    dbContext.Database.EnsureCreated();
//}

//app.Run();
using CycleApp.DataAccess;
using CycleApp.Middleware;
using CycleApp.Services;
using CycleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CycleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICodeStorageService, CodeStorageService>();
builder.Services.AddScoped<ICycleCalculatorService, CycleCalculatorService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins != null && origins.Length > 0)
        {
            policyBuilder.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
        }
    });
});

builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CycleDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
