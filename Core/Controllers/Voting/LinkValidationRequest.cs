using System.ComponentModel.DataAnnotations;

namespace Core.Controllers.Voting;

public class LinkValidationRequest
{
    [Required]
    public string AudioUrl { get; set; }
}