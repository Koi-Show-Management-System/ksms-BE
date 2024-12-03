using Autofac;
using KSMS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace KSMS.API.Configuration
{
    public static class ConfigureDbContext
    {
        public static IServiceCollection AddDbContext(this IServiceCollection services)
        {
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(AppConfig.ConnectionString.DefaultConnection));
            return services;
        }

        public static ContainerBuilder AddDbContext(this ContainerBuilder builder)
        {
            //builder.Register(c => new SqlConnection(AppConfig.ConnectionString.DefaultConnection))
                    //.As<IDbConnection>()
                    //.InstancePerLifetimeScope();

            //builder.RegisterType<ApplicationDbContext>().As<DbContext>().InstancePerLifetimeScope();
            return builder;
        }
    }
}
