using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PClub.Api.Controllers;
using PClub.Api.Infrastructure;
using PClub.Application;
using PClub.Application.Zones;
using PClub.Infrastructure;
using PClub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration, builder.Environment.IsDevelopment());
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Валидатор ставки объявлен на интерфейсе IHasZonePrice, а фильтр ищет
// валидатор по КОНКРЕТНОМУ типу аргумента — связываем явно. Без этой
// строки валидатор молча не сработает: AddValidatorsFromAssembly
// зарегистрировал его как IValidator<IHasZonePrice>.
builder.Services.AddScoped<
    IValidator<ChangeZonePriceRequest>, ChangeZonePriceValidator>();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Вот зачем был GenerateDocumentationFile: эти две строки забирают
    // комментарии /// и показывают их в Swagger как описания эндпоинтов.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;

        context.ProblemDetails.Instance ??=
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
    };
});
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();
app.MapControllers();

app.Run();
