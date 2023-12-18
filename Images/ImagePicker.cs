using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Images;

public interface IImageLoader : IHostedService
{
    Task<Image?> GetCurrent();
}

public class ImageLoader : IImageLoader
{
    private const string BucketName = "post-radio";

    private Image? _current;
    private AmazonS3Client _client;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync("spaces-credentials.json", cancellationToken);
        var credentials = JsonConvert.DeserializeObject<SpacesCredentials>(json);
        
        var config = new AmazonS3Config()
        {
            ServiceURL = "https://fra1.digitaloceanspaces.com",
            ForcePathStyle = true
        };
        
        _client = new AmazonS3Client(credentials.AccessKey, credentials.SecretKey, config);
        
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
        var listResponse = await _client.ListObjectsAsync(BucketName);
        var files = listResponse.S3Objects;
        var random = new Random();
        var index = random.Next(files.Count);

        string fileName = files[index].Key;

        var preSignedUrlRequest = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = fileName,
            Expires = DateTime.Now.AddMinutes(10) // Link will expire in 5 minutes
        };

        var url = await _client.GetPreSignedURLAsync(preSignedUrlRequest);
        
        Console.WriteLine(url + "\n\n\n");

        return new Image(url);
    }
}

public class Image
{
    public Image(string link)
    {
        Link = link;
    }

    public readonly string Link;
}