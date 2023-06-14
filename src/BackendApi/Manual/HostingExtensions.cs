using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace BackendApi.Manual;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddAuthorization(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .Build();
            
            options.DefaultPolicy = policy;
            options.FallbackPolicy = policy;
            
            options.AddPolicy("Read", p => p.RequireRole("weather.app.read"));
        });

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Manual:Authority"];
                options.Audience = builder.Configuration["Manual:Audience"];
                //options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                options.TokenValidationParameters.RoleClaimType = "roles";
            });
            
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();
        
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllers().RequireAuthorization();

        return app;
    }
}
