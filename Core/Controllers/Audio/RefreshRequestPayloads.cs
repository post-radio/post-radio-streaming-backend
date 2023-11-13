using System.ComponentModel.DataAnnotations;

namespace Core.Controllers.Audio;

public class RefreshRequestRequest
{
    [Required]
    public string Key { get; set; }
}

[Serializable]
public class RefreshRequestResponse
{
    public string Result { get; set; }
}