using Core.Configs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var configurationRoot = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json")
    .Build();

ConfigureSettings();
await ConfigureRedis();

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
return;

void ConfigureSettings()
{
    if (services == null || configurationRoot == null)
        throw new NullReferenceException();

    var redisConfig = configurationRoot.GetSection("Redis").Get<RedisConfig>();
    var folderStructureConfig = configurationRoot.GetSection("FoldersStructure").Get<FoldersStructure>();

    if (redisConfig == null || folderStructureConfig == null)
        throw new NullReferenceException();

    Console.WriteLine(folderStructureConfig.AudioFolder);

    services.AddSingleton(redisConfig);
    services.AddSingleton(folderStructureConfig);

    if (Directory.Exists(folderStructureConfig.AudioFolder) == false)
        Directory.CreateDirectory(folderStructureConfig.AudioFolder);
}

async Task ConfigureRedis()
{
    var redisConfig = configurationRoot.GetSection("Redis").Get<RedisConfig>();

    var config = new ConfigurationOptions
    {
        EndPoints = { redisConfig.RedisEndpoint }
    };

    var connection = await ConnectionMultiplexer.ConnectAsync(config);
    var db = connection.GetDatabase();

    if (redisConfig.RedisFlushDbOnStartup == true)
        db.Execute("FLUSHDB");

    services.AddSingleton(db);
}