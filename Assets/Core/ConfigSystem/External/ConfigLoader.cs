using System.Collections.Generic;
using Core.AssetManagement.Runtime;
using Core.ConfigSystem.Runtime;
using UnityEngine;
using Zenject;

namespace Core.ConfigSystem.External
{
    public class ConfigLoader : IConfigLoader, IInitializable
    {
        [Inject] private IAssetLoader m_AssetLoader;
        [Inject] private IConfigReader m_ConfigReader;

        private Dictionary<string, object> m_LoadedConfigs = new Dictionary<string, object>();

        public void Initialize()
        {
            Debug.Log("ConfigController: Initialized");
        }

        public T GetConfig<T>(string configName) where T : class
        {
            if (m_LoadedConfigs.TryGetValue(configName, out var cachedConfig))
            {
                return cachedConfig as T;
            }

            var config = LoadAndParseConfig<T>(configName);
            if (config != null)
            {
                m_LoadedConfigs[configName] = config;
            }

            return config;
        }

        public bool IsConfigLoaded(string configName)
        {
            return m_LoadedConfigs.ContainsKey(configName);
        }

        public void PreloadConfig(string configName)
        {
            if (IsConfigLoaded(configName))
            {
                return;
            }

            try
            {
                LoadConfigJson(configName);
                Debug.Log($"ConfigController: Preloaded {configName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ConfigController: Failed to preload {configName} - {ex.Message}");
            }
        }

        private T LoadAndParseConfig<T>(string configName) where T : class
        {
            try
            {
                var jsonText = LoadConfigJson(configName);
                if (string.IsNullOrEmpty(jsonText))
                {
                    Debug.LogError($"ConfigController: Empty JSON for {configName}");
                    return null;
                }

                var result = m_ConfigReader.Deserialize<T>(jsonText);
                if (!result.IsExist)
                {
                    Debug.LogError($"ConfigController: Failed to parse {configName}");
                    return null;
                }

                Debug.Log($"ConfigController: Successfully loaded and parsed {configName}");
                return result.Object;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ConfigController: Error loading config {configName} - {ex.Message}");
                return null;
            }
        }

        private string LoadConfigJson(string configName)
        {
            var textAsset = m_AssetLoader.LoadSync<TextAsset>(configName);
            if (textAsset == null)
            {
                Debug.LogError($"ConfigController: Failed to load TextAsset {configName}");
                return null;
            }

            return textAsset.text;
        }
    }
}