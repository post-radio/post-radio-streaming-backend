using Audio.Entities;

namespace Audio.Controllers.Voting;

[Serializable]
public class LinkValidationResponse
{
    public bool IsValid { get; set; }
    public TrackMetadata?  Metadata { get; set; }
}