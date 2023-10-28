using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers;

[ApiController]
[Route("audio")]
public class AudioLinksController : ControllerBase
{
    [HttpGet()]
    public async Task<AudioLinkResponse> Get()
    {
        return new AudioLinkResponse() { AudioLink = "PisaPopaKaka" };
    }
}