using Core.Configs;
using StackExchange.Redis;

namespace Core.Services;

public static class ServicesExtensions
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPlaylistProvider, PlaylistProvider>();
    }
    
    public static async Task AddRedis(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        
        var configurationRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json")
            .Build();
        
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
        services.AddSingleton<IPlaylistProvider, PlaylistProvider>();
    }
    
    public static void ConfigureBuilder(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
    
    public static async Task ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        await app.Services.GetRequiredService<IPlaylistProvider>().Setup();
    }
}