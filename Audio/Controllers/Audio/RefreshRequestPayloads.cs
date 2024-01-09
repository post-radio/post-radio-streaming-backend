using System.ComponentModel.DataAnnotations;
using Audio.Services;

namespace Audio.Controllers.Audio;

public class RefreshRequestRequest
{
    [Required]
    public string Key { get; set; }
}

[Serializable]
public class RefreshRequestResponse
{
    public PlaylistRefreshResult[] Result { get; set; }
}