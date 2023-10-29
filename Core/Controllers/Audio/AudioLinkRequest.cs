using System.ComponentModel.DataAnnotations;

namespace Core.Controllers.Audio;

public class AudioLinkRequest
{
    [Required]
    public string AudioUrl { get; set; }
}