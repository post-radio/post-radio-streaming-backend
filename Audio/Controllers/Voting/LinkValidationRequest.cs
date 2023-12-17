using System.ComponentModel.DataAnnotations;

namespace Audio.Controllers.Voting;

public class LinkValidationRequest
{
    [Required]
    public string AudioUrl { get; set; }
}