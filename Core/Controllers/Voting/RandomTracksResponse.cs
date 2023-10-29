using Core.Entities;

namespace Core.Controllers.Voting;

[Serializable]
public class RandomTracksResponse
{
    public TrackMetadata[] Tracks { get; set; }
}