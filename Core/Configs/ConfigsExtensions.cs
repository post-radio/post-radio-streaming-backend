namespace Core.Configs;

public static class ConfigsExtensions
{
    public static void AddConfigs(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        
        var configurationRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json")
            .Build();
        
        var redisConfig = configurationRoot.GetSection("Redis").Get<RedisConfig>();
        var folderStructureConfig = configurationRoot.GetSection("FoldersStructure").Get<FoldersStructure>();
        var playlistConfig = configurationRoot.GetSection("Playlist").Get<PlaylistConfig>();
        
        if (redisConfig == null || folderStructureConfig == null || playlistConfig == null)
            throw new NullReferenceException();

        services.AddSingleton(redisConfig);
        services.AddSingleton(folderStructureConfig);
        services.AddSingleton(playlistConfig);

        if (Directory.Exists(folderStructureConfig.AudioFolder) == false)
            Directory.CreateDirectory(folderStructureConfig.AudioFolder);
    }
}