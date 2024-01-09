using SoundCloudExplode.Tracks;

namespace Audio.Entities;

public static class EntitiesExtensions
{
    public static TrackMetadata? ToMetadata(this Track track)
    {
        if (track.Title == null || track.PermalinkUrl == null)
            return null;

        string name = string.Empty;
        string author = string.Empty;

        if (track.Title.Contains(" - "))
        {
            var splitTitle = track.Title.Split(" - ");
            name = splitTitle[0];
            author = splitTitle[1];
        }
        else if (track.Description != null)
        {
            name = track.Title;
            author = track.Description;
        }
        else if (track.PublisherMetadata is { Artist: not null })
        {
            name = track.Title;
            author = track.PublisherMetadata.Artist;
        }

        int duration;

        if (track.Duration != null)
            duration = (int)track.Duration;
        else
            duration = 100000;

        if (duration == 0)
            throw new NullReferenceException();

        return new TrackMetadata()
        {
            Url = track.PermalinkUrl.ToString(),
            Author = author,
            Name = name,
            Duration = duration
        };
    }

    public static int GetDuration(this Track track)
    {
        if (track.Duration != null)
            return (int)track.Duration;
        
        return 100000;
    }
}