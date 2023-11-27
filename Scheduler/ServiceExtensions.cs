using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingSystem.Entities;
using BookingSystem.Repositories;

namespace ASPNETCoreScheduler.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureMySqlContext(this IServiceCollection services, IConfiguration config)
        {
            var connectionString =  config["ConnectionStrings:DefaultConnection"];
            services.AddDbContext<AppDb>(o => o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); //original #db init
        }

        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddSingleton<IRepositoryWrapper, RepositoryWrapper>();
        }
    }
}
