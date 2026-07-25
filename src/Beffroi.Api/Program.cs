using Beffroi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Génération du document OpenAPI par Microsoft.AspNetCore.OpenApi (natif .NET 10).
builder.Services.AddOpenApi();

// Branchement de l'hexagone : dispatcher CQRS, handlers du cœur, adapters secondaires.
builder.Services.AddBeffroiInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Expose le document sur /openapi/v1.json ...
    app.MapOpenApi();

    // ... et l'UI Swagger par-dessus (Swashbuckle n'est utilisé QUE pour l'UI, pas la génération).
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Beffroi API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
