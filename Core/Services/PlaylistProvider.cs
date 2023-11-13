using Core.Configs;
using Core.Entities;
using SoundCloudExplode;
using SoundCloudExplode.Common;

namespace Core.Services;

public interface IPlaylistProvider
{
    Task Setup();
    IReadOnlyList<TrackMetadata> GetTracks();
    Task<int> Refresh();
}

public class PlaylistProvider : IPlaylistProvider
{
    private readonly PlaylistConfig _config;
    private readonly SoundCloudClient _client = new();

    private IReadOnlyList<TrackMetadata> _tracks;

    public PlaylistProvider(PlaylistConfig config)
    {
        _config = config;
    }

    public async Task Setup()
    {
        var tracks = await _client.Playlists.GetTracksAsync(_config.Url);
        var resultTracks = new List<TrackMetadata>();

        foreach (var track in tracks)
        {
            var metadata = track.ToMetadata();
            
            if (metadata == null)
                continue;
            
            resultTracks.Add(metadata);
        }

        Console.WriteLine($"Tracks found: {resultTracks.Count}");
        
        _tracks = resultTracks;
    }

    public IReadOnlyList<TrackMetadata> GetTracks()
    {
        return _tracks;
    }

    public async Task<int> Refresh()
    {
        var tracks = await _client.Playlists.GetTracksAsync(_config.Url);
        var resultTracks = new List<TrackMetadata>();

        foreach (var track in tracks)
        {
            var metadata = track.ToMetadata();
            
            if (metadata == null)
                continue;
            
            resultTracks.Add(metadata);
        }

        Console.WriteLine($"Tracks found: {resultTracks.Count}");
        
        _tracks = resultTracks;

        return resultTracks.Count;
    }
}