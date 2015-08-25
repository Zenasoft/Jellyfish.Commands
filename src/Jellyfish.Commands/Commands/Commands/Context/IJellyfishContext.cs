namespace Jellyfish.Commands
{
    public interface IJellyfishContext
    {
        RequestCache<T> GetCache<T>(string commandName);
        RequestLog GetRequestLog();
    }
}