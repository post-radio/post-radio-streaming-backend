using Audio.Configs;

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
        
        var folderStructureConfig = configurationRoot.GetSection("FoldersStructure").Get<FoldersStructure>();
        var playlistConfig = configurationRoot.GetSection("Playlists").Get<PlaylistConfig>();
        
        if (folderStructureConfig == null || playlistConfig == null)
            throw new NullReferenceException();

        services.AddSingleton(folderStructureConfig);
        services.AddSingleton(playlistConfig);

        if (Directory.Exists(folderStructureConfig.AudioFolder) == false)
            Directory.CreateDirectory(folderStructureConfig.AudioFolder);
    }
}