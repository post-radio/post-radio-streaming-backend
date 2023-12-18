using Microsoft.AspNetCore.Mvc;

namespace Images;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    public ImagesController(IImageLoader loader)
    {
        _loader = loader;
    }

    private readonly IImageLoader _loader;

    [HttpGet("random")]
    public async Task<string> GetRandom()
    {
        var image = await _loader.GetCurrent();

        if (image == null)
            return "not-found";
        
        return image.Link;
    }
}