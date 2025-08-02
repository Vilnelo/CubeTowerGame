using Core.AssetManagement.Runtime;
using Core.Canvases.Runtime;
using Core.Scene.Runtime;
using UnityEngine;
using Zenject;

namespace Core.Scene.External
{
    public class StartSceneController : MonoBehaviour, IInitializable
    {
        [Inject] private ISceneController m_SceneController;
        [Inject] private IMainCanvas m_MainCanvas;
        [Inject] private IAssetLoader m_AssetLoader;

        private const string m_StartButtonPrefabAddress = "StartButtonView";
        private const string m_StartButtonTextKey = "START GAME";

        private StartButtonView m_StartButtonView;

        public void Initialize()
        {
            Debug.Log("StartSceneController initialized");
            LoadAndSetupUI();
        }

        private void LoadAndSetupUI()
        {
            try
            {
                Debug.Log($"Loading StartButton prefab: {m_StartButtonPrefabAddress}");

                m_StartButtonView = m_AssetLoader.InstantiateSync<StartButtonView>(
                    m_StartButtonPrefabAddress,
                    m_MainCanvas.GetTransform()
                );

                if (m_StartButtonView == null)
                {
                    Debug.LogError($"Failed to load StartButtonView: {m_StartButtonPrefabAddress}");
                    return;
                }

                SetupButtonView();
                Debug.Log("StartButton UI loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading StartButton UI: {ex.Message}");
            }
        }

        private void SetupButtonView()
        {
            if (m_StartButtonView == null)
            {
                return;
            }

            if (m_StartButtonView.Text != null)
            {
                //TODO: Добавить метод .Localize()
                m_StartButtonView.Text.text = m_StartButtonTextKey;
            }

            if (m_StartButtonView.Button != null)
            {
                m_StartButtonView.Button.onClick.AddListener(OnStartButtonClicked);
            }
        }

        private void OnStartButtonClicked()
        {
            Debug.Log("Start button clicked!");

            if (m_StartButtonView?.Button != null)
            {
                m_StartButtonView.Button.interactable = false;
            }

            m_SceneController.LoadScene(SceneController.CoreSceneName);
        }

        void OnDestroy()
        {
            if (m_StartButtonView?.Button != null)
            {
                m_StartButtonView.Button.onClick.RemoveAllListeners();
            }

            m_AssetLoader.Release("start_button_view");
        }
    }
}