namespace Core.Configs;

public class RedisConfig
{
    public required string RedisEndpoint { get; set; }

    public bool RedisFlushDbOnStartup { get; set; }
}