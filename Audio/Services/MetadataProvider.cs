using Audio.Configs;
using Audio.Entities;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SoundCloudExplode;
using SoundCloudExplode.Common;

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
    private const string _dataPath = "playlist-metadata.json";

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
        var json = await File.ReadAllTextAsync(_dataPath);
        var metadatas = JsonConvert.DeserializeObject<Dictionary<string, TrackMetadata>>(json);

        var tracks = await _soundCloud.Playlists.GetTracksAsync(_config.Url);

        foreach (var track in tracks)
        {
            var data = track.ToMetadata();

            if (data == null || metadatas.ContainsKey(data.Url) == true)
                continue;

            metadatas.Add(data.Url, data);
        }
        
        _metadata.Clear();
        
        foreach (var (id, data) in metadatas)
            _metadata.Add(id, data);

        var resultMeta = _metadata.OrderBy(t => t.Value.Author);
        var resultDictionary = new Dictionary<string, TrackMetadata>();

        foreach (var (url, data) in resultMeta)
            resultDictionary.Add(url, data);
        
        var resultObject = JsonConvert.SerializeObject(resultDictionary);
        
        await File.WriteAllTextAsync(_dataPath, resultObject);
    }

    public bool TryGetMetadata(string url, out TrackMetadata? metadata)
    {
        if (_metadata.TryGetValue(url, out metadata) == false)
            return false;

        return true;
    }
}