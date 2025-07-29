using Core.Scene.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Scene.External
{
    public class StartSceneController : MonoBehaviour, IInitializable
    {
        [SerializeField] private Button m_StartButton;
    
        [Inject] private ISceneController m_SceneController;

        public void Initialize()
        {
            Debug.Log("StartSceneController initialized");
            SetupUI();
        }
    
        private void SetupUI()
        {
            if (m_StartButton != null)
            {
                m_StartButton.onClick.AddListener(OnStartButtonClicked);
            }
        }
    
        private void OnStartButtonClicked()
        {
            Debug.LogError("Кликаем на кнопку");
            m_SceneController.LoadScene(SceneController.CoreSceneName);
        }
    
        void OnDestroy()
        {
            if (m_StartButton != null)
            {
                m_StartButton.onClick.RemoveAllListeners();
            }
        }
    }
}
