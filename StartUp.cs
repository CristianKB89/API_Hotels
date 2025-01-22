using API_Hotels.DataContext;
using API_Hotels.Repositories;
using API_Hotels.Repositories.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;


[assembly: FunctionsStartup(typeof(API_Hotels.StartUp))]

namespace API_Hotels
{
    public class StartUp : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<DapperContext>();

            #region Repositories

            builder.Services.AddTransient<IHotelManagementService, HotelManagementService>();

            #endregion Repositories

        }

        public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
        {
            public override OpenApiInfo Info { get; set; } = new OpenApiInfo
            {
                Title = "API_Hotels",
                Version = "1.0.0",
                Description = "API para la gestión de hoteles y reservas",
                Contact = new OpenApiContact
                {
                    Name = "Cristian Brijaldo",
                    Email = "cristianiksl@gmail.com",
                }
            };

            public override List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>
        {
            new OpenApiServer
            {
                Url = "https://apihotels-production.up.railway.app/api",
                Description = "Production Server"
            }
        };
        }
    }
}
