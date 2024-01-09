using Audio.Configs;
using Audio.Services;
using Core.Configs;
using Microsoft.AspNetCore.Mvc;
using SoundCloudExplode;

namespace Audio.Controllers.Audio;

[ApiController]
[Route("api/audio")]
public class AudioController : ControllerBase
{
    public AudioController(
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

    [HttpGet("link")]
    public async Task<AudioLinkResponse> GetLink([FromQuery] AudioLinkRequest request)
    {
        var track = await Soundcloud.Tracks.GetAsync(request.AudioUrl);

        if (track == null)
            throw new NullReferenceException();

        var id = track.Id.ToString();
        var filePath = $"{_foldersStructure.AudioFolder}{id}.mp3";

        if (System.IO.File.Exists(filePath))
            return new AudioLinkResponse() { AudioUrl = filePath };

        await Soundcloud.DownloadAsync(track, filePath);

        return new AudioLinkResponse() { AudioUrl = filePath };
    }
    
    [HttpGet("refresh")]
    public async Task<RefreshRequestResponse> Refresh([FromQuery] RefreshRequestRequest request)
    {
        if (request.Key != _credentials.ApiKey)
            return new RefreshRequestResponse() { Result = Array.Empty<PlaylistRefreshResult>() };

        var tracksCount = await _playlistProvider.Refresh();

        return new RefreshRequestResponse() { Result = tracksCount.ToArray() };
    }
}