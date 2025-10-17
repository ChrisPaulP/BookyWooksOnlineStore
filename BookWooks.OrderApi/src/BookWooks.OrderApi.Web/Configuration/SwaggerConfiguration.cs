using Microsoft.OpenApi.Models;
using System.Reflection;

namespace BookWooks.OrderApi.Web.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(swg =>
        {
            swg.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Microservice", Version = "v1" });
            swg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
            });
            swg.AddSecurityRequirement(new OpenApiSecurityRequirement 
            { 
                { 
                    new OpenApiSecurityScheme 
                    { 
                        Reference = new OpenApiReference 
                        { 
                            Type = ReferenceType.SecurityScheme, 
                            Id = "Bearer" 
                        } 
                    }, 
                    Array.Empty<string>() 
                } 
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            swg.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
