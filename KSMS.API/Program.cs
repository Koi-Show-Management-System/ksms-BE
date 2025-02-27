using KSMS.API.Configuration;
using KSMS.API.Middlewares;
using System.Text.Json.Serialization;
using KSMS.Domain.Common;
using Net.payOS;
using KSMS.Application.Repositories;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Repositories;
using KSMS.Infrastructure.Services;
using KSMS.Infrastructure.Hubs;
using KSMS.Application.Services;
using Microsoft.Extensions.Caching.Distributed;

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
builder.Services.AddHostedService<ShowStatusBackgroundService>();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.ConfigureAutofacContainer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", 
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

builder.Services.AddMvc()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
ConfigureFireBase.AddFirebase();
var app = builder.Build();

// BỎ đoạn if (app.Environment.IsDevelopment()) và thay bằng đoạn sau:
// Configure the HTTP request pipeline.
// Bật Swagger cho cả môi trường Production và Development
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KSMS API v1");
    //c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.Full);
    // c.RoutePrefix = string.Empty; // Uncomment nếu muốn đặt Swagger làm trang chủ
});

app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseMiddleware<ConfirmationTokenMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ScoreHub>("/scoreHub");
app.MapHub<NotificationHub>("/notificationHub");

await app.RunAsync();