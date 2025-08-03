using Utils.Result;

namespace Core.ConfigSystem.Runtime
{
    public interface IConfigReader
    {
        Result<T> Deserialize<T>(string json);
    }
}