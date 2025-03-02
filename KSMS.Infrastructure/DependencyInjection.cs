using Autofac;
using KSMS.Application.Repositories;
using KSMS.Infrastructure.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using KSMS.Domain.Dtos.Requests.KoiProfile;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Entities;

namespace KSMS.Infrastructure
{
    public static class DependencyInjection
    {

        public static void RegisterServices(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.Register(ctx =>
            {
                var storageClient = StorageClient.Create();
                return storageClient;
            }).SingleInstance();
        }
        public static void RegisterRepository(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(GenericRepository<>))
                    .As(typeof(IGenericRepository<>)).InstancePerDependency();

            builder.RegisterGeneric(typeof(UnitOfWork<>))
                .As(typeof(IUnitOfWork<>)).InstancePerDependency();
        }
        public static void RegisterMapster(this ContainerBuilder builder)
        {
            var config = new TypeAdapterConfig();
            config.Scan(Assembly.GetExecutingAssembly());
            //update koi
            TypeAdapterConfig<UpdateKoiProfileRequest, KoiProfile>.NewConfig().IgnoreNullValues(true);
            //update show
            TypeAdapterConfig<UpdateShowRequestV2, KoiShow>.NewConfig().IgnoreNullValues(true);
            builder.RegisterInstance(config).AsSelf().SingleInstance();
            builder.RegisterType<Mapper>().As<IMapper>().InstancePerLifetimeScope();
        }
    }
}
