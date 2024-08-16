using Logging;
using Serilog;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog(SeriLogger.Configure);

builder.Services.AddAuthentication()
    .AddJwtBearer("IdentityApiKey", options =>
    {
        options.Authority = builder.Configuration["IdentityServerURL"];
        options.Audience = "resource_ocelot";
        options.RequireHttpsMetadata = false; // Set to true in production

    });
builder.Services.AddOcelot();
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName.ToLower()}.json", true, true);

var app = builder.Build();

await app.UseOcelot();

app.Run();