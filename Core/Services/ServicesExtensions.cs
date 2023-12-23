using Audio.Services;
using Images;
using SoundCloudExplode;

namespace Core.Services;

public static class ServicesExtensions
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMetadataProvider, MetadataProvider>();
        builder.Services.AddSingleton<IPlaylistProvider, PlaylistProvider>();
        builder.Services.AddSingleton<IImageLoader, ImageLoader>();
        builder.Services.AddHostedService<IImageLoader>(sp => sp.GetRequiredService<IImageLoader>());
        builder.Services.AddHostedService<IMetadataProvider>(sp => sp.GetRequiredService<IMetadataProvider>());
        builder.Services.AddHostedService<IPlaylistProvider>(sp => sp.GetRequiredService<IPlaylistProvider>());
        var soundCloud = new SoundCloudClient();
        builder.Services.AddSingleton(soundCloud);
    }

    public static void ConfigureBuilder(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public static void ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}