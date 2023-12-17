using System.ComponentModel.DataAnnotations;

namespace Audio.Controllers.Audio;

public class AudioLinkRequest
{
    [Required]
    public string AudioUrl { get; set; }
}

[Serializable]
public class AudioLinkResponse
{
    public string AudioUrl { get; set; }
}