using EcoFleet.API.Middlewares;
using EcoFleet.Application;
using EcoFleet.Infrastructure;
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
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 4. Configure Middleware Pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // <--- Our Custom Error Handler

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging(); // <--- Log HTTP requests cleanly

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
