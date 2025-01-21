using Autofac;
using KSMS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Data;
using KSMS.Infrastructure.Database;
using Microsoft.Data.SqlClient;

namespace KSMS.API.Configuration
{
    public static class ConfigureDbContext
    {
        public static IServiceCollection AddDbContext(this IServiceCollection services)
        {
            services.AddDbContext<KoiShowManagementSystemContext>(options => options.UseSqlServer(AppConfig.ConnectionString.DefaultConnection));
            return services;
        }

        public static ContainerBuilder AddDbContext(this ContainerBuilder builder)
        {
            builder.Register(c => new SqlConnection(AppConfig.ConnectionString.DefaultConnection))
                    .As<IDbConnection>()
                    .InstancePerLifetimeScope();

            builder.RegisterType<KoiShowManagementSystemContext>().As<DbContext>().InstancePerLifetimeScope();
            return builder;
        }
    }
}
