using Core.Configs;
using Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();
builder.AddConfigs();
builder.ConfigureBuilder();

var app = builder.Build();
await app.ConfigureApp();
app.Run();