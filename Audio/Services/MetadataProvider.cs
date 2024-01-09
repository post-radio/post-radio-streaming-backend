using Audio.Configs;
using Audio.Entities;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SoundCloudExplode;
using SoundCloudExplode.Common;
using SoundCloudExplode.Tracks;

namespace Audio.Services;

public interface IMetadataProvider : IHostedService
{
    Task Refresh();
    bool TryGetMetadata(string url, out TrackMetadata? metadata);
}

public class MetadataProvider : IMetadataProvider
{
    public MetadataProvider(SoundCloudClient soundCloud, PlaylistConfig config)
    {
        _soundCloud = soundCloud;
        _config = config;
        _metadata = new Dictionary<string, TrackMetadata>();
    }

    private readonly SoundCloudClient _soundCloud;
    private readonly PlaylistConfig _config;
    private readonly Dictionary<string, TrackMetadata> _metadata;
    private const string _pathPostfix = "-playlist-metadata.json";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Refresh();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task Refresh()
    {
        foreach (var (name, _) in _config.Urls)
        {
            var path = $"{name}{_pathPostfix}";
            var json = await File.ReadAllTextAsync(path);
            var metadatas = JsonConvert.DeserializeObject<Dictionary<string, TrackMetadata>>(json);

            var tracks = await GetTracks(name);

            foreach (var track in tracks)
            {
                var data = track.ToMetadata();

                if (data == null)
                    continue;

                metadatas.TryAdd(data.Url, data);
            }

            _metadata.Clear();

            foreach (var (id, data) in metadatas)
                _metadata.Add(id, data);

            var resultDictionary = new Dictionary<string, TrackMetadata>();

            foreach (var (url, data) in _metadata)
                resultDictionary.Add(url, data);

            var resultObject = JsonConvert.SerializeObject(resultDictionary);

            await File.WriteAllTextAsync(path, resultObject);
        }
    }

    public bool TryGetMetadata(string url, out TrackMetadata? metadata)
    {
        if (_metadata.TryGetValue(url, out metadata) == false)
            return false;

        return true;
    }

    private async Task<IReadOnlyList<Track>> GetTracks(string playlistName)
    {
        var url = _config.Urls[playlistName];
        var tracks = await _soundCloud.Playlists.GetTracksAsync(url);

        return tracks;
    }
}