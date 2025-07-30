using Core.AssetManagement.Runtime;
using Core.Canvases.Runtime;
using UnityEngine;
using Zenject;

namespace Core.UI.External
{
    public class CoreUIController : IInitializable
    {
        [Inject] private IAssetLoader m_AssetLoader;
        [Inject] private IMainCanvas m_MainCanvas;
        
        public const string m_CoreUIPrefabAddress = "CoreUIView";

        private CoreUIView m_CoreUIView;
        
        public void Initialize()
        {
            Debug.Log("CoreUIController: Initializing...");
            LoadCoreUI();
        }

        public CoreUIView GetCoreView()
        {
            return m_CoreUIView;
        }
        
        private void LoadCoreUI()
        {
            try
            {
                m_CoreUIView = m_AssetLoader.InstantiateSync<CoreUIView>(
                    m_CoreUIPrefabAddress,
                    m_MainCanvas.GetTransform()
                );

                if (m_CoreUIView == null)
                {
                    Debug.LogError("CoreUIController: Failed to load CoreUIView");
                    return;
                }

                Debug.Log("CoreUIController: CoreUI loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"CoreUIController: Error loading CoreUI - {ex.Message}");
            }
        }
    }
}