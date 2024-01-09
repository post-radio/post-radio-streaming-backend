using Audio.Entities;

namespace Audio.Services;

public class PlaylistHandler
{
    public PlaylistHandler(IReadOnlyList<TrackMetadata> tracks, int outputTracksAmount)
    {
        Tracks = tracks;
        _outputTracksAmount = outputTracksAmount;
    }

    private readonly Queue<TrackMetadata> _queue = new();
    private readonly int _outputTracksAmount;
    private TrackMetadata _currentHead;
    
    public readonly IReadOnlyList<TrackMetadata> Tracks;

    public async Task IterateQueue()
    {
        while (true)
        {
            if (_queue.Count == 0)
                FillQueue();
            
            _currentHead = _queue.Dequeue();
            
            await Task.Delay(_currentHead.Duration);
        }
    }

    public IReadOnlyList<TrackMetadata> GetRandom()
    {
        if (Tracks.Count < _outputTracksAmount)
            throw new ArgumentOutOfRangeException();
        
        var all = new List<TrackMetadata>(Tracks);
        var tracks = new List<TrackMetadata>();
        var random = new Random();
        
        all.Remove(_currentHead);
        tracks.Add(_currentHead);
        
        for (var i = 0; i < _outputTracksAmount - 1; i++)
        {
            var index = random.Next(0, all.Count);
            tracks.Add(all[index]);
            all.RemoveAt(index);
        }

        return tracks;
    }
    
    private void FillQueue()
    {
        var tracks = new List<TrackMetadata>(Tracks);
        var random = new Random();

        while (tracks.Count > 0)
        {
            var index = random.Next(tracks.Count);
            _queue.Enqueue(tracks[index]);
            tracks.RemoveAt(index);
        }
    }
}