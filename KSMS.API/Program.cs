using KSMS.API.Configuration;
using KSMS.API.Middlewares;
using System.Text.Json.Serialization;
using Hangfire;
using KSMS.Application.Extensions;
using KSMS.Domain.Common;
using Net.payOS;
using KSMS.Application.Repositories;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Repositories;
using KSMS.Infrastructure.Services;
using KSMS.Infrastructure.Hubs;
using KSMS.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var payOs = new PayOS(builder.Configuration["PayOs:ClientId"]!,
    builder.Configuration["PayOs:ApiKey"]!,
    builder.Configuration["PayOs:ChecksumKey"]!);
builder.Services.AddSingleton(payOs);
builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddHostedService<ShowStatusBackgroundService>();
builder.Services.AddSwaggerGen();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.SchemaFilter<EnumSchemaFilter>();
// });
builder.Services.AddHttpContextAccessor();
builder.ConfigureAutofacContainer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000",
                "http://localhost:8081",
                "https://ksms.news",
                "https://www.ksms.news",
                "https://api.ksms.news")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Cần thiết cho SignalR
    });
});
builder.Services.AddScoped<IUnitOfWork<KoiShowManagementSystemContext>, UnitOfWork<KoiShowManagementSystemContext>>();
builder.Configuration.SettingsBinding();
builder.Services.AddAuthenticationServicesConfigurations(builder.Configuration);
builder.Services.AddSwaggerConfigurations();
builder.Services.AddDbContext();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddHttpClient<ILivestreamService, LivestreamService>();
builder.Services.AddMvc()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
    options.Queues = ["default"];
    options.ServerTimeout = TimeSpan.FromMinutes(5);
    options.ShutdownTimeout = TimeSpan.FromMinutes(2);
    options.ServerCheckInterval = TimeSpan.FromMinutes(2);
    options.SchedulePollingInterval = TimeSpan.FromSeconds(2);
});
ConfigureFireBase.AddFirebase();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KSMS API v1");
});

app.UseCors("AllowAll");
app.UseHangfireDashboard("/hangfire");
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseMiddleware<ConfirmationTokenMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ScoreHub>("/scoreHub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<VoteHub>("/voteHub");
app.MapHub<LivestreamHub>("/livestreamHub");

await app.RunAsync();

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumDescriptions = Enum.GetValues(context.Type)
                .Cast<Enum>()
                .ToDictionary(e => e.ToString(), e => e.GetDescription());

            schema.Enum.Clear();
            foreach (var description in enumDescriptions.Values)
            {
                schema.Enum.Add(new OpenApiString(description));
            }
        }
    }
}