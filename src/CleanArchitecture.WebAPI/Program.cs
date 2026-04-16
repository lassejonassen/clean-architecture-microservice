using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDefaults(string.Empty);

builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

app.MapControllers();
app.UseDefaults();

app.Run();