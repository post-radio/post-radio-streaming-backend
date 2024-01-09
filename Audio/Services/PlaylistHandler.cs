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
    private List<TrackMetadata> _currentRandom;
    
    public readonly IReadOnlyList<TrackMetadata> Tracks;
    public IReadOnlyList<TrackMetadata> CurrentRandom => _currentRandom;

    public async Task IterateQueue()
    {
        while (true)
        {
            var random = new List<TrackMetadata>();

            for (var i = 0; i < _outputTracksAmount; i++)
            {
                if (_queue.Count == 0)
                    FillQueue();

                random.Add(_queue.Dequeue());
            }

            _currentRandom = random;

            var first = random.First();
            await Task.Delay(first.Duration);
        }
    }

    public IReadOnlyList<TrackMetadata> GetRandom()
    {
        if (Tracks.Count - 1 < _outputTracksAmount)
            throw new ArgumentOutOfRangeException();
        
        var all = new List<TrackMetadata>(Tracks);
        
        var tracks = new List<TrackMetadata>();
        var head = Dequeue();
        tracks.Add(head);
        
        var random = new Random();
        
        for (var i = 0; i < _outputTracksAmount - 1; i++)
        {
            var index = random.Next(0, all.Count);
            tracks.Add(all[index]);
            all.RemoveAt(index);
        }

        return tracks;
    }
    
    private TrackMetadata Dequeue()
    {
        if (_queue.Count == 0)
            FillQueue();

        return _queue.Dequeue();
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