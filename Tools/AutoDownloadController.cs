using Audio.Configs;
using Audio.Controllers.Audio;
using Audio.Entities;
using Audio.Services;
using Core.Configs;
using Microsoft.AspNetCore.Mvc;
using SoundCloudExplode;

namespace Tools;

[ApiController]
[Route("api/tools")]
public class AutoDownloadController : ControllerBase
{
    public AutoDownloadController(
        IPlaylistProvider playlistProvider,
        FoldersStructure foldersStructure,
        Credentials credentials)
    {
        _playlistProvider = playlistProvider;
        _foldersStructure = foldersStructure;
        _credentials = credentials;
    }

    private readonly IPlaylistProvider _playlistProvider;
    private readonly FoldersStructure _foldersStructure;
    private readonly Credentials _credentials;

    private static readonly SoundCloudClient Soundcloud = new();

    [HttpGet("downloadAll")]
    public async Task<string> GetLink()
    {
        var allTracks = _playlistProvider.GetAll();
        var counter = 0;

        var tasks = new List<Task>();
        
        foreach (var metadata in allTracks)
            tasks.Add(Download(metadata));

        await Task.WhenAll(tasks);
        Console.WriteLine($"All {allTracks.Count} downloaded");
        return $"Total track loaded: {allTracks.Count}";

        async Task Download(TrackMetadata metadata)
        {
            counter++;
            Console.WriteLine($"Tracks downloaded: {counter} / {allTracks.Count}");

            var filePath = $"{_foldersStructure.AudioFolder}{counter}.mp3";

            if (System.IO.File.Exists(filePath))
                return;

            var track = await Soundcloud.Tracks.GetAsync(metadata.Url);
            await Soundcloud.DownloadAsync(track, filePath);
        }
    }
}