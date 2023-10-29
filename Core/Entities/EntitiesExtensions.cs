using SoundCloudExplode.Tracks;

namespace Core.Entities;

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

            return new TrackMetadata() { Url = track.PermalinkUrl.ToString(), Author = author, Name = name };
        }

        if (track.Description != null)
        {
            name = track.Title;
            author = track.Description;

            return new TrackMetadata() { Url = track.PermalinkUrl.ToString(), Author = author, Name = name };
        }
        
        if (track.PublisherMetadata != null)
        {
            name = track.Title;
            author = track.PublisherMetadata.Artist;

            return new TrackMetadata() { Url = track.PermalinkUrl.ToString(), Author = author, Name = name };
        }

        Console.WriteLine("Failed to find track");
        
        return null;
    }
}