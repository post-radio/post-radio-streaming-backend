using Audio.Configs;
using Audio.Entities;
using Microsoft.Extensions.Hosting;
using SoundCloudExplode;
using SoundCloudExplode.Common;
using SoundCloudExplode.Tracks;

namespace Audio.Services;

public interface IPlaylistProvider : IHostedService
{
    IReadOnlyList<TrackMetadata> GetRandom(string[] playlists);
    IReadOnlyList<TrackMetadata> GetAll();
    Task<IReadOnlyList<PlaylistRefreshResult>> Refresh();
}

public class PlaylistRefreshResult
{
    public PlaylistRefreshResult(string name, int tracksCount)
    {
        Name = name;
        TracksCount = tracksCount;
    }
    
    public string Name { get; }
    public int TracksCount { get; }
}

public class PlaylistProvider : IPlaylistProvider
{
    public PlaylistProvider(
        PlaylistConfig config,
        SoundCloudClient soundCloud,
        IMetadataProvider metadataProvider)
    {
        _config = config;
        _soundCloud = soundCloud;
        _metadataProvider = metadataProvider;
    }

    private readonly PlaylistConfig _config;
    private readonly SoundCloudClient _soundCloud;
    private readonly IMetadataProvider _metadataProvider;

    private readonly Dictionary<string, PlaylistHandler> _playlists = new();
    private readonly List<PlaylistHandler> _sourcePlaylists = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Refresh();

        foreach (var (_, playlist) in _playlists)
            Task.Run(() => playlist.IterateQueue());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IReadOnlyList<TrackMetadata> GetRandom(string[] playlists)
    {
        playlists = playlists.Order().ToArray();
        var name = playlists[0];

        for (var i = 1; i < playlists.Length; i++)
            name += $"/{playlists[i]}";

        return _playlists[name].GetRandom();
    }

    public IReadOnlyList<TrackMetadata> GetAll()
    {
        var result = new List<TrackMetadata>();

        foreach (var playlist in _sourcePlaylists)
            result.AddRange(playlist.Tracks);

        return result;
    }

    public async Task<IReadOnlyList<PlaylistRefreshResult>> Refresh()
    {
        await _metadataProvider.Refresh();
        
        foreach (var (_, playlist) in _playlists)
            playlist.Dispose();
        
        _playlists.Clear();
        
        var playlists = new Dictionary<string, PlaylistHandler>();
        var playlistsNames = _config.Urls.Keys.Order().ToList();
        var results = new List<PlaylistRefreshResult>();
        
        foreach (var name in playlistsNames)
        {
            var playlist = await CreatePlaylist(name);
            playlists.Add(name, playlist);
            results.Add(new PlaylistRefreshResult(name, playlist.Tracks.Count));
        }

        foreach (var (_, playlist) in playlists)
            _sourcePlaylists.Add(playlist);

        for (var i = 0; i < playlistsNames.Count - 1; i++)
        {
            var toMerge = new List<PlaylistHandler>();
            var name = string.Empty;
            var headName = playlistsNames[i];
            toMerge.Add(playlists[headName]);
            name += headName;

            for (var j = i + 1; j < playlistsNames.Count; j++)
            {
                var toMergeName = playlistsNames[j];
                name += $"/{toMergeName}";
                toMerge.Add(playlists[toMergeName]);
                var merged = await CreateMergedPlaylist(toMerge);
                playlists.Add(name, merged);
                results.Add(new PlaylistRefreshResult(name, merged.Tracks.Count));
            }
        }

        foreach (var (name, playlist) in playlists)
            _playlists.Add(name, playlist);

        foreach (var (_, playlist) in _playlists)
            Task.Run(() => playlist.IterateQueue());
        
        return results;
    }

    private async Task<PlaylistHandler> CreateMergedPlaylist(IReadOnlyList<PlaylistHandler> playlists)
    {
        var allTracks = new List<TrackMetadata>();

        foreach (var playlist in playlists)
            allTracks.AddRange(playlist.Tracks);

        
        var randomTracks = new List<TrackMetadata>();
        var random = new Random();

        while (allTracks.Count != 0)
        {
            var index = random.Next(0, allTracks.Count);
            var track = allTracks[index];
            randomTracks.Add(track);
            allTracks.RemoveAt(index);
        }

        var mergedPlaylist = new PlaylistHandler(randomTracks, _config.RandomTracksAmount);
        return mergedPlaylist;
    }

    private async Task<PlaylistHandler> CreatePlaylist(string name)
    {
        var tracks = await GetTracks(name);
        var metadatas = new List<TrackMetadata>();

        foreach (var track in tracks)
        {
            if (track.PermalinkUrl == null)
                continue;

            if (_metadataProvider.TryGetMetadata(track.PermalinkUrl.ToString(), out var metadata) == false)
                metadata = track.ToMetadata();

            if (metadata == null)
                continue;

            if (metadata.Duration == 0)
                metadata.Duration = track.GetDuration();

            metadatas.Add(metadata);
        }

        var playlist = new PlaylistHandler(metadatas, _config.RandomTracksAmount);

        return playlist;
    }

    private async Task<IReadOnlyList<Track>> GetTracks(string playlist)
    {
        var url = _config.Urls[playlist];
        var tracks = await _soundCloud.Playlists.GetTracksAsync(url);

        return tracks;
    }
}