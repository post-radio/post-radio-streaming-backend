using Core.Configs;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using SoundCloudExplode;

namespace Core.Controllers;

[ApiController]
[Route("api/audio")]
public class AudioLinksController : ControllerBase
{
    private readonly IDatabase _database;
    private readonly FoldersStructure _foldersStructure;

    private static readonly SoundCloudClient Soundcloud = new();

    public AudioLinksController(IDatabase database, FoldersStructure foldersStructure)
    {
        _database = database;
        _foldersStructure = foldersStructure;
    }

    [HttpGet]
    public async Task<AudioLinkResponse> Get(AudioLinkRequest request)
    {
        Console.WriteLine($"Requested vide: {request.VideoUrl}");
        Console.WriteLine($"targetFolder: {_foldersStructure.AudioFolder}");

        var track = await Soundcloud.Tracks.GetAsync(request.VideoUrl);

        if (track == null)
            throw new NullReferenceException();

        var id = track.Id;  
        var filePath = $"{_foldersStructure.AudioFolder}{id}.mp3";

        Console.WriteLine(filePath);
        await Soundcloud.DownloadAsync(track, filePath);

        await StoreInRedis(request.VideoUrl, id.ToString());

        return new AudioLinkResponse() { AudioLink = filePath };
    }

    private async Task StoreInRedis(string videoUrl, string audioFilePath)
    {
        await _database.StringSetAsync(videoUrl, audioFilePath);
    }
}