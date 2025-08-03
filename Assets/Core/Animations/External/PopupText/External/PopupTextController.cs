using System;
using Core.AssetManagement.Runtime;
using Core.UI.Runtime;
using UnityEngine;
using Zenject;

namespace Core.Animations.External.PopupText.External
{
    public class PopupTextController : IInitializable, IDisposable
    {
        [Inject] private IAssetLoader m_AssetLoader;
        [Inject] private ICoreUIController m_CoreUIController;

        private const string m_PopupTextPrefabAddress = "PopupTextView";

        private PopupTextAnimation m_PopupTextAnimation;
        private PopupTextView m_PopupTextView;
        private RectTransform m_SpawnParent;

        // Событие для показа текста
        public static event Action<string> OnShowPopupText;

        public void Initialize()
        {
            Debug.Log("PopupTextController: Initializing...");
            
            LoadPopupTextPrefab();
            SetupSpawnParent();
            SubscribeToEvents();
        }

        private void LoadPopupTextPrefab()
        {
            try
            {
                // Загружаем GameObject, а не PopupTextView напрямую
                var prefabGameObject = m_AssetLoader.LoadSync<GameObject>(m_PopupTextPrefabAddress);
                
                if (prefabGameObject == null)
                {
                    Debug.LogError("PopupTextController: Failed to load popup text prefab");
                    return;
                }

                // Проверяем, что на GameObject'е есть компонент PopupTextView
                var popupTextViewComponent = prefabGameObject.GetComponent<PopupTextView>();
                if (popupTextViewComponent == null)
                {
                    Debug.LogError("PopupTextController: Prefab doesn't have PopupTextView component");
                    return;
                }

                // Создаем экземпляр объекта и получаем PopupTextView компонент
                var instantiatedGameObject = UnityEngine.Object.Instantiate(prefabGameObject);
                m_PopupTextView = instantiatedGameObject.GetComponent<PopupTextView>();
                
                if (m_PopupTextView == null)
                {
                    Debug.LogError("PopupTextController: Failed to get PopupTextView component from instantiated object");
                    UnityEngine.Object.Destroy(instantiatedGameObject);
                    return;
                }
                
                Debug.Log("PopupTextController: Popup text prefab loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PopupTextController: Error loading popup text prefab - {ex.Message}");
            }
        }

        private void SetupSpawnParent()
        {
            try
            {
                var coreView = m_CoreUIController.GetCoreView();
                if (coreView == null)
                {
                    Debug.LogError("PopupTextController: CoreUIView is null");
                    return;
                }

                // Получаем место для спавна (вы сами создадите этот элемент в CoreUIView)
                m_SpawnParent = coreView.BottomBlocksView.GetPopupTextParrent();
                
                if (m_SpawnParent == null)
                {
                    Debug.LogError("PopupTextController: Spawn parent is null");
                    return;
                }

                if (m_PopupTextView == null)
                {
                    Debug.LogError("PopupTextController: PopupTextView is null, cannot initialize animation");
                    return;
                }

                // Инициализируем анимацию
                m_PopupTextAnimation = new PopupTextAnimation();
                m_PopupTextAnimation.Initialize(m_PopupTextView, m_SpawnParent);

                Debug.Log("PopupTextController: Spawn parent setup completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PopupTextController: Error setting up spawn parent - {ex.Message}");
            }
        }

        private void SubscribeToEvents()
        {
            OnShowPopupText += HandleShowPopupText;
            Debug.Log("PopupTextController: Subscribed to events");
        }

        private void HandleShowPopupText(string text)
        {
            if (m_PopupTextAnimation == null)
            {
                Debug.LogError("PopupTextController: Animation not initialized");
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("PopupTextController: Empty text provided");
                return;
            }

            Debug.Log($"PopupTextController: Showing popup text: {text}");
            m_PopupTextAnimation.PlayAnimation(text);
        }

        // Статический метод для удобного вызова из любого места
        public static void ShowPopupText(string text)
        {
            OnShowPopupText?.Invoke(text);
        }

        // Метод для принудительной остановки анимации
        public void StopCurrentAnimation()
        {
            if (m_PopupTextAnimation != null)
            {
                m_PopupTextAnimation.ForceStop();
            }
        }

        public bool IsAnimationPlaying()
        {
            return m_PopupTextAnimation?.IsAnimating() ?? false;
        }

        public void Dispose()
        {
            Debug.Log("PopupTextController: Disposing...");
            
            OnShowPopupText -= HandleShowPopupText;
            
            m_PopupTextAnimation?.Dispose();
            
            if (m_PopupTextView != null)
            {
                UnityEngine.Object.Destroy(m_PopupTextView.gameObject);
            }
        }
    }
}