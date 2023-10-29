using System.ComponentModel.DataAnnotations;

namespace Core.Controllers;

public class AudioLinkRequest
{
    [Required]
    public string VideoUrl { get; set; }
}