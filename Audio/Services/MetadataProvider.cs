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
    public MetadataProvider(
        SoundCloudClient soundCloud,
        PlaylistConfig config,
        FoldersStructure foldersStructure)
    {
        _soundCloud = soundCloud;
        _config = config;
        _foldersStructure = foldersStructure;
        _metadata = new Dictionary<string, TrackMetadata>();
    }

    private readonly SoundCloudClient _soundCloud;
    private readonly PlaylistConfig _config;
    private readonly FoldersStructure _foldersStructure;
    private readonly Dictionary<string, TrackMetadata> _metadata;
    private const string PathPostfix = "-playlist-metadata.json";

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
        _metadata.Clear();

        foreach (var (name, _) in _config.Urls)
        {
            var path = $"{_foldersStructure.AudioFolder}{name}{PathPostfix}";
            
            Console.WriteLine($"Refresh meta for: {name} path: {path}");
            if (File.Exists(path) == false)
            {
                var tmp = JsonConvert.SerializeObject(new Dictionary<string, TrackMetadata>());

                await File.WriteAllTextAsync(path, tmp);
            }
            
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

            foreach (var (url, data) in metadatas)
                _metadata.TryAdd(url, data);

            var resultObject = JsonConvert.SerializeObject(metadatas);

            await File.WriteAllTextAsync(path, resultObject);
            Console.WriteLine($"Refresh meta for: {name} path: {path}");
        }
        
        Console.WriteLine($"Total tracks in metadata: {_metadata.Count}");
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