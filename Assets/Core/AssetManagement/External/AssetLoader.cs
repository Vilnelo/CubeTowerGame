using System.Collections.Generic;
using Core.AssetManagement.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.AssetManagement.External
{
    public class AssetLoader : IAssetLoader
    {
        private readonly Dictionary<string, Object> m_LoadedAssets = new();
        
        private readonly Dictionary<string, List<GameObject>> m_InstantiatedObjects = new();

        public T LoadSync<T>(string address) where T : Object
        {
            if (m_LoadedAssets.TryGetValue(address, out var cachedAsset))
            {
                Debug.Log($"[AssetLoader] Found in cache: {address}");
                return cachedAsset as T;
            }

            try
            {
                var asset = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();

                if (asset != null)
                {
                    m_LoadedAssets[address] = asset;
                    Debug.Log($"[AssetLoader] Loaded: {address}");
                }
                else
                {
                    Debug.LogError($"[AssetLoader] Failed to load: {address}");
                }

                return asset;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AssetLoader] Error loading {address}: {ex.Message}");
                return null;
            }
        }

        public T InstantiateSync<T>(string address, Transform parent = null) where T : Component
        {
            var prefab = LoadSync<GameObject>(address);
            if (prefab == null) 
            {
                Debug.LogError($"[AssetLoader] Cannot instantiate - prefab not loaded: {address}");
                return null;
            }

            return CreateInstance<T>(address, prefab, parent);
        }

        public T InstantiateSync<T>(string address, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            var prefab = LoadSync<GameObject>(address);
            if (prefab == null) 
            {
                Debug.LogError($"[AssetLoader] Cannot instantiate - prefab not loaded: {address}");
                return null;
            }

            var instance = Object.Instantiate(prefab, position, rotation, parent);
            TrackInstance(address, instance);

            if (instance.TryGetComponent<T>(out var component))
            {
                Debug.Log($"[AssetLoader] Instantiated: {address}");
                return component;
            }

            Debug.LogError($"[AssetLoader] Component {typeof(T)} not found on {address}");
            Object.Destroy(instance);
            return null;
        }

        private T CreateInstance<T>(string address, GameObject prefab, Transform parent) where T : Component
        {
            var instance = Object.Instantiate(prefab, parent);
            TrackInstance(address, instance);

            if (instance.TryGetComponent<T>(out var component))
            {
                Debug.Log($"[AssetLoader] Instantiated: {address}");
                return component;
            }

            Debug.LogError($"[AssetLoader] Component {typeof(T)} not found on {address}");
            Object.Destroy(instance);
            return null;
        }

        private void TrackInstance(string address, GameObject instance)
        {
            if (!m_InstantiatedObjects.ContainsKey(address))
            {
                m_InstantiatedObjects[address] = new List<GameObject>();
            }
            m_InstantiatedObjects[address].Add(instance);
        }

        public bool IsLoaded(string address)
        {
            return m_LoadedAssets.ContainsKey(address);
        }

        public void Release(string address)
        {
            if (m_InstantiatedObjects.TryGetValue(address, out var instances))
            {
                foreach (var instance in instances)
                {
                    if (instance != null)
                        Object.Destroy(instance);
                }
                m_InstantiatedObjects.Remove(address);
            }
            
            if (m_LoadedAssets.TryGetValue(address, out var asset))
            {
                Addressables.Release(asset);
                m_LoadedAssets.Remove(address);
                Debug.Log($"[AssetLoader] Released: {address}");
            }
        }

        public void ReleaseAll()
        {
            foreach (var kvp in m_InstantiatedObjects)
            {
                foreach (var instance in kvp.Value)
                {
                    if (instance != null)
                        Object.Destroy(instance);
                }
            }
            
            foreach (var kvp in m_LoadedAssets)
            {
                Addressables.Release(kvp.Value);
            }
            
            m_LoadedAssets.Clear();
            m_InstantiatedObjects.Clear();
            Debug.Log("[AssetLoader] Released all assets");
        }
    }
}