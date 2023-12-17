using System.Net;
using System.Net.Http.Headers;
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
    public async Task<HttpResponseMessage> GetRandom()
    {
        var image = await _loader.GetCurrent();
        
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new ByteArrayContent(image.Raw);
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(image.MimeType);

        Console.WriteLine($"Response: {image.Raw.Length} {image.MimeType}");
        
        return response;
    }
    
    [HttpGet("verify")]
    public async Task<bool> Verify()
    {
        return true;
    }
}