using Core.Configs;
using Core.Entities;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using SoundCloudExplode;
using SoundCloudExplode.Tracks;

namespace Core.Controllers.Voting;

[ApiController]
[Route("api/voting")]
public class VotingController : ControllerBase
{
    public VotingController(IPlaylistProvider playlist, PlaylistConfig config)
    {
        _playlist = playlist;
        _config = config;
    }

    private readonly IPlaylistProvider _playlist;
    private readonly PlaylistConfig _config;
    private readonly Random _random = new();
    private static readonly SoundCloudClient Soundcloud = new();

    [HttpGet("randomList")]
    public async Task<RandomTracksResponse> GetRandomList()
    {
        var tracks = _playlist.GetTracks();

        var tracksIds = new List<int>();
        var selectedTracks = new List<TrackMetadata>();

        for (var i = 0; i < tracks.Count; i++)
            tracksIds.Add(i);

        for (var i = 0; i < _config.RandomTracksAmount; i++)
        {
            var random = _random.Next(0, tracksIds.Count);
            var trackId = tracksIds[random];
            tracksIds.RemoveAt(random);

            var track = tracks[trackId];
            selectedTracks.Add(track);
        }

        return new RandomTracksResponse() { Tracks = selectedTracks.ToArray() };
    }

    [HttpGet("validate")]
    public async Task<LinkValidationResponse> Validate(LinkValidationRequest request)
    {
        Track? track;
        try
        {
            track = await Soundcloud.Tracks.GetAsync(request.AudioUrl);
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