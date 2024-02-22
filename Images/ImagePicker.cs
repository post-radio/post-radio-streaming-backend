using Microsoft.Extensions.Hosting;
using Shared;

namespace Images;

public interface IImageLoader : IHostedService
{
    Task<Image?> GetCurrent();
}

public class ImageLoader : IImageLoader
{
    public ImageLoader(FoldersStructure foldersStructure)
    {
        _foldersStructure = foldersStructure;
    }

    private const string Domain = "https://streaming.post-radio.io/images/";
    
    private readonly FoldersStructure _foldersStructure;
    private readonly List<string> _images = new();
    private Image? _current;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(async () => await RunLoop(), cancellationToken);
        return Task.CompletedTask;
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
            _current = LoadRandom();
            await Task.Delay(25000);
        }
    }

    private Image LoadRandom()
    {
        if (_images.Count == 0)
            FillQueue();

        var random = new Random();
        var index = random.Next(_images.Count);
        var fileName = _images[index];
        var url = $"{Domain}{fileName}";

        return new Image(url);
    }

    private void FillQueue()
    {
        _images.Clear();

        var files = Directory.GetFiles(_foldersStructure.ImagesFoled, "*.*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName).ToList();

        _images.AddRange(files);
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