using API_Hotels.DataContext;
using API_Hotels.Repositories;
using API_Hotels.Repositories.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;


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
    }
}
