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

    private readonly Queue<S3Object> _queue = new();
     
    private Image? _current;
    private AmazonS3Client _client;
    private DateTime _expirationTime => DateTime.Now.AddMinutes(3f);

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
            await Task.Delay(25000);
        }
    }

    private async Task<Image> LoadRandom()
    {
        if (_queue.Count == 0)
            await FillQueue();
        
        var fileName = _queue.Dequeue().Key;

        var preSignedUrlRequest = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = fileName,
            Expires = _expirationTime
        };

        try
        {
            var url = await _client.GetPreSignedURLAsync(preSignedUrlRequest);
            return new Image(url);
        }
        catch
        {
            Console.WriteLine($"Could not find image: {fileName}");
            return new Image(string.Empty);
        }
    }

    private async Task FillQueue()
    {
        var listResponse = await _client.ListObjectsV2Async(new ListObjectsV2Request() { BucketName = BucketName });
        var files = listResponse.S3Objects;
        
        var random = new Random();

        while (files.Count > 0)
        {
            var index = random.Next(files.Count);
            _queue.Enqueue(files[index]);
            files.RemoveAt(index);
        }
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