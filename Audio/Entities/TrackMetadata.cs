using System.Text.Json.Serialization;

namespace Audio.Entities;

[Serializable]
public class TrackMetadata
{
    public string Url { get; set; }
    public string Author { get; set; }
    public string Name { get; set; }
    
    [Newtonsoft.Json.JsonIgnore] [JsonIgnore]
    public int Duration { get; set; }
}