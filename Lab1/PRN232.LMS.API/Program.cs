using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Seed;
using PRN232.LMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Avoid accidental cycles if a DTO ever references back.
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LmsDb")));

builder.Services.AddLmsServices();

// Dev-only CORS to make Swagger/clients work smoothly during local testing.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(options =>
{
    options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
        };
    });
});
app.UseSwaggerUI();

// Ensure DB exists + seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    await db.Database.EnsureCreatedAsync();
    await LmsDbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
