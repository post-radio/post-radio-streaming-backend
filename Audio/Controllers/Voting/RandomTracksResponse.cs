using Audio.Entities;

namespace Audio.Controllers.Voting;

[Serializable]
public class RandomTracksResponse
{
    public TrackMetadata[] Tracks { get; set; }
}