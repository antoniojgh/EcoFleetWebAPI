using EcoFleet.API.Middlewares;
using EcoFleet.Application;
using EcoFleet.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// 2. Add Services (Layers)
// These extension methods come from the DependencyInjection.cs files we created in each layer
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Add API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "EcoFleet API",
            Version = "v1",
            Description = "Fleet management REST API for EcoFleet — manage vehicles, drivers, and assignments."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// 4. Configure Middleware Pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // <--- Our Custom Error Handler

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("EcoFleet API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseSerilogRequestLogging(); // <--- Log HTTP requests cleanly

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
