using Audio.Configs;
using Audio.Entities;
using Microsoft.Extensions.Hosting;
using SoundCloudExplode;
using SoundCloudExplode.Common;

namespace Audio.Services;

public interface IPlaylistProvider : IHostedService
{
    IReadOnlyList<TrackMetadata> GetRandom(int amount);
    Task<int> Refresh();
}

public class PlaylistProvider : IPlaylistProvider
{
    private readonly PlaylistConfig _config;
    private readonly SoundCloudClient _client = new();
    private readonly Queue<TrackMetadata> _queue = new();

    private IReadOnlyList<TrackMetadata>? _tracks;

    public PlaylistProvider(PlaylistConfig config)
    {
        _config = config;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Refresh();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IReadOnlyList<TrackMetadata> GetRandom(int amount)
    {
        if (_queue.Count < amount)
            FillQueue();

        var selectedTracks = new List<TrackMetadata>();

        for (var i = 0; i < _config.RandomTracksAmount; i++)
            selectedTracks.Add(_queue.Dequeue());

        return selectedTracks;
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

        _tracks = resultTracks;

        return resultTracks.Count;
    }

    private void FillQueue()
    {
        if (_tracks == null)
            throw new NullReferenceException();

        var tracks = new List<TrackMetadata>(_tracks);
        var random = new Random();

        while (tracks.Count > 0)
        {
            var index = random.Next(tracks.Count);
            _queue.Enqueue(tracks[index]);
            tracks.RemoveAt(index);
        }
    }
}