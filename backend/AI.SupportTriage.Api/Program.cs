using System.Text.Json.Serialization;
using AI.SupportTriage.Api.Infrastructure;
using AI.SupportTriage.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TriageExceptionHandler>();
builder.Services.AddTriageServices(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddPolicy("LocalFrontend", policy =>
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "https://localhost:3000",
                    "http://127.0.0.1:3000",
                    "https://127.0.0.1:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()));
}

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("LocalFrontend");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
