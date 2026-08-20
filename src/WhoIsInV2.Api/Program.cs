using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using WhoIsInV2.Api.Auth;
using WhoIsInV2.Application.Authentication;
using WhoIsInV2.Application.Common.Interfaces;
using WhoIsInV2.Application.Users;
using WhoIsInV2.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().WriteTo.Console());
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddCors(options => options.AddPolicy("WebDev", policy => policy.WithOrigins("http://localhost:5173", "https://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
    builder.Services.AddScoped<IAccessTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt settings are not configured.");
    if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32) throw new InvalidOperationException("Jwt signing key must be configured and at least 32 characters long.");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwtOptions.Issuer, ValidateAudience = true, ValidAudience = jwtOptions.Audience, ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)), ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    });
    builder.Services.AddAuthorization();
    builder.Services.AddOpenApi();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();
    if (app.Environment.IsDevelopment()) { app.MapOpenApi(); app.UseSwaggerUI(options => options.SwaggerEndpoint("openapi/v1.json", "WhosIn")); }
    app.UseSerilogRequestLogging();
    app.UseCors("WebDev");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Application terminated unexpectedly"); }
finally { Log.CloseAndFlush(); }