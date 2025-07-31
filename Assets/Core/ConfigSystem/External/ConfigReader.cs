using Core.ConfigSystem.Runtime;
using Newtonsoft.Json;
using UnityEngine;
using Utils.Result;

namespace Core.ConfigSystem.External
{
    public class ConfigReader : IConfigReader
    {
        public Result<T> Deserialize<T>(string json)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(json);
                return new Result<T>(result, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ConfigJsonReader: Failed to deserialize {typeof(T).Name} - {ex.Message}");
                return new Result<T>(default(T), false);
            }
        }
    }
}