using KSMS.API.Configuration;
using KSMS.API.Middlewares;
using System.Text.Json.Serialization;
using KSMS.Domain.Common;
using Net.payOS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var payOs = new PayOS(builder.Configuration["PayOs:ClientId"]!,
    builder.Configuration["PayOs:ApiKey"]!,
    builder.Configuration["PayOs:ChecksumKey"]!); builder.Services.AddSingleton(payOs);
builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.ConfigureAutofacContainer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Configuration.SettingsBinding();
builder.Services.AddAuthenticationServicesConfigurations(builder.Configuration);
builder.Services.AddSwaggerConfigurations();
builder.Services.AddDbContext();
builder.Services.AddMvc()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
ConfigureFireBase.AddFirebase();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseMiddleware<ConfirmationTokenMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
