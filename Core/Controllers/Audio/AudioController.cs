using Core.Configs;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using SoundCloudExplode;
using StackExchange.Redis;

namespace Core.Controllers.Audio;

[ApiController]
[Route("api/audio")]
public class AudioController : ControllerBase
{
    public AudioController(
        IDatabase database, 
        IPlaylistProvider playlistProvider,
        FoldersStructure foldersStructure)
    {
        _database = database;
        _playlistProvider = playlistProvider;
        _foldersStructure = foldersStructure;
    }

    private readonly IDatabase _database;
    private readonly IPlaylistProvider _playlistProvider;
    private readonly FoldersStructure _foldersStructure;

    private const string SecurityKey = "912JopaJopa01";

    private static readonly SoundCloudClient Soundcloud = new();

    [HttpGet("link")]
    public async Task<AudioLinkResponse> GetLink([FromQuery] AudioLinkRequest request)
    {
        var track = await Soundcloud.Tracks.GetAsync(request.AudioUrl);

        if (track == null)
            throw new NullReferenceException();

        var id = track.Id.ToString();
        var filePath = $"{_foldersStructure.AudioFolder}{id}.mp3";

        var existingAudioLink = await _database.StringGetAsync(id);

        if (existingAudioLink.IsNullOrEmpty == false)
            return new AudioLinkResponse() { AudioUrl = existingAudioLink.ToString() };

        await Soundcloud.DownloadAsync(track, filePath);
        await _database.StringSetAsync(id, filePath);

        return new AudioLinkResponse() { AudioUrl = filePath };
    }
    
    [HttpGet("refresh")]
    public async Task<RefreshRequestResponse> Refresh([FromQuery] RefreshRequestRequest request)
    {
        if (request.Key != SecurityKey)
            return new RefreshRequestResponse() { Result = "Wrong security key provided" };

        var tracksCount = await _playlistProvider.Refresh();

        return new RefreshRequestResponse() { Result = $"Tracks found: {tracksCount}" };
    }
}