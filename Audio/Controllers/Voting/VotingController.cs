using Audio.Configs;
using Audio.Entities;
using Audio.Services;
using Microsoft.AspNetCore.Mvc;
using SoundCloudExplode;
using SoundCloudExplode.Tracks;

namespace Audio.Controllers.Voting;

[ApiController]
[Route("api/voting")]
public class VotingController : ControllerBase
{
    public VotingController(SoundCloudClient client, IPlaylistProvider playlist, PlaylistConfig config)
    {
        _client = client;
        _playlist = playlist;
        _config = config;
    }

    private readonly SoundCloudClient _client;
    private readonly IPlaylistProvider _playlist;
    private readonly PlaylistConfig _config;

    [HttpGet("randomList")]
    public Task<RandomTracksResponse> GetRandomList()
    {
        var selectedTracks = _playlist.GetRandom(_config.RandomTracksAmount);
        var response = new RandomTracksResponse() { Tracks = selectedTracks.ToArray() };
        return Task.FromResult(response);
    }

    [HttpGet("validate")]
    public async Task<LinkValidationResponse> Validate([FromQuery] LinkValidationRequest request)
    {
        Track? track;
        try
        {
            track = await _client.Tracks.GetAsync(request.AudioUrl);
        }
        catch
        {
            return new LinkValidationResponse() { IsValid = false };
        }

        if (track == null)
            return new LinkValidationResponse() { IsValid = false };

        return new LinkValidationResponse() { IsValid = true, Metadata = track.ToMetadata() };
    }
}