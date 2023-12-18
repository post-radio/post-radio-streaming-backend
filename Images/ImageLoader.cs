using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Hosting;

namespace Images;

public interface IImageLoader : IHostedService
{
    Task<Image?> GetCurrent();
}

public class ImageLoader : IImageLoader
{
    private DriveService _driveService;
    private Image? _current;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        GoogleCredential credential;
        await using (var stream = new FileStream("google-drive-credentials.json", FileMode.Open, FileAccess.Read))
        {
            credential = (await GoogleCredential.FromStreamAsync(stream, cancellationToken))
                .CreateScoped(DriveService.Scope.DriveReadonly);
        }

        _driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "post-radio"
        });
        
        Task.Run(async () => await RunLoop());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<Image?> GetCurrent()
    {
        return Task.FromResult(_current);
    }

    private async Task RunLoop()
    {
        await Task.Delay(5000);
        
        while (true)
        {
            _current = await LoadRandom();
            await Task.Delay(5000);
        }
    }

    private async Task<Image> LoadRandom()
    {
        var request = _driveService.Files.List();
        request.Q = "'1IP-T1Cvsdj3oMgLXxwqfZdUo50NDe5_V' in parents";
        request.Fields = "nextPageToken, files(id, name, mimeType, webContentLink)";

        var fileList = (await request.ExecuteAsync()).Files;

        var random = new Random();
        var randomIndex = random.Next(0, fileList.Count);
        var randomImage = fileList[randomIndex];

        try
        {
            var imageBytes = await _driveService.HttpClient.GetByteArrayAsync(randomImage.WebContentLink);

            var stream = new MemoryStream(imageBytes);
            Console.WriteLine($"{stream.ToArray()} {randomImage.MimeType}");
            return new Image(stream.ToArray(), randomImage.MimeType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}

public class Image
{
    public Image(byte[] raw, string mimeType)
    {
        Raw = raw;
        MimeType = mimeType;
    }

    public readonly byte[] Raw;
    public readonly string MimeType;
}