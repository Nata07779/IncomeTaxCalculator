using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TaskCalculator.Domain.Interfaces;
using TaxCalculator.Api.JWT;
using TaxCalculator.Data;
using TaxCalculator.Services;

namespace TaxCalculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            // Set db
            builder.Services.AddDbContext<Data.AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                ));

            // Identity
            builder.Services
                .AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                // User settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            // JWT authentication
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
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection("Jwt"));

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();

            // Register calculators and selector (Strategy pattern)
            builder.Services.AddScoped<ITaxCalculator, ZeroTaxCalculator>();
            builder.Services.AddScoped<ITaxCalculator, ProgressiveTaxCalculator>();
            builder.Services.AddScoped<ITaxCalculatorSelector, TaxCalculatorSelector>();
            // Strategy wrapper is the one exposed as ITaxCalculator
            builder.Services.AddScoped<IStrategyTaxCalculator, StrategyTaxCalculator>();

            builder.Services.AddScoped<ITaxBandRepository, TaxBandRepository>();

            builder.Services.AddScoped<JwtTokenService>();

            // health checks
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(opt =>        // configure Swagger to save token between requests  
            {
                opt.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Please provide the Bearer token",
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                opt.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            var env = app.Environment;

            // Global error handling middleware - must be early in pipeline
            app.UseMiddleware<TaxCalculator.Middleware.ErrorHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Health endpoints
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
                    });
                    await context.Response.WriteAsync(result);
                }
            });

            app.MapPost("/login",
                async (
                    LoginRequest request,
                    UserManager<IdentityUser> userManager,
                    JwtTokenService tokenService) =>
                {
                    var user = await userManager.FindByEmailAsync(request.Email);

                    if (user == null ||
                        !await userManager.CheckPasswordAsync(user, request.Password))
                    {
                        return Results.Unauthorized();
                    }

                    var token = tokenService.CreateToken(user);

                    return Results.Ok(new { access_token = token });
                });

            app.MapPost("/register",
                async (RegisterRequest request, UserManager<IdentityUser> userManager) =>
                {
                    var user = new IdentityUser
                    {
                        UserName = request.Email,
                        Email = request.Email
                    };

                    var result = await userManager.CreateAsync(user, request.Password);

                    return result.Succeeded
                        ? Results.Ok()
                        : Results.BadRequest(result.Errors);
                });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
