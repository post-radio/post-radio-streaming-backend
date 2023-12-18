using Audio.Services;
using Core.Configs;
using Images;
using StackExchange.Redis;

namespace Core.Services;

public static class ServicesExtensions
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPlaylistProvider, PlaylistProvider>();
        builder.Services.AddSingleton<IImageLoader, ImageLoader>();
        builder.Services.AddHostedService<IImageLoader>(sp => sp.GetRequiredService<IImageLoader>());
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