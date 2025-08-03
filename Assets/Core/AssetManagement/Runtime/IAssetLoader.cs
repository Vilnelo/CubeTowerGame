using UnityEngine;

namespace Core.AssetManagement.Runtime
{
    public interface IAssetLoader
    {
        T LoadSync<T>(string address) where T : Object;
        T InstantiateSync<T>(string address, Transform parent = null) where T : Component;

        T InstantiateSync<T>(string address, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component;

        bool IsLoaded(string address);

        void Release(string address);
        void ReleaseAll();
    }
}