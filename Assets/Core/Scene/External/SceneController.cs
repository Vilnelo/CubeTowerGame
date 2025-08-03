using System.Collections;
using Core.Scene.Runtime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Scene.External
{
    public class SceneController : MonoBehaviour, ISceneController
    {
        public const string StartSceneName = "StartScene";
        public const string CoreSceneName = "CoreScene";

        private bool m_IsLoadingNow;
        private bool m_IsLoadingComplete;
        private string m_SceneNameForLoad;

        public string ActiveScene => SceneManager.GetActiveScene().name;
        public bool IsLoadingNow => m_IsLoadingNow;
        public bool IsLoadingComplete => m_IsLoadingComplete;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            LogDebug("SceneController initialized and set as DontDestroyOnLoad");
        }

        void Start()
        {
            LogDebug($"SceneController started on scene: {ActiveScene}");
        }

        public void LoadScene(string sceneName)
        {
            Debug.Log($"Loading scene '{sceneName}'");
            if (m_IsLoadingNow)
            {
                LogDebug($"Scene loading already in progress. Ignoring request for: {sceneName}");
                return;
            }

            if (ActiveScene == sceneName)
            {
                LogDebug($"Scene {sceneName} is already active. Ignoring load request.");
                return;
            }

            LogDebug($"Starting to load scene: {sceneName}");
            m_SceneNameForLoad = sceneName;
            ChangeScene(m_SceneNameForLoad);
        }

        public void Reload()
        {
            LogDebug($"Reloading current scene: {ActiveScene}");
            LoadScene(ActiveScene);
        }

        private void ChangeScene(string sceneName)
        {
            if (m_IsLoadingNow)
            {
                LogDebug("Scene change already in progress");
                return;
            }

            LogDebug($"Changing scene to: {sceneName}");

            DOTween.KillAll();

            m_IsLoadingNow = true;
            m_IsLoadingComplete = false;

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            LogDebug($"Starting async load of scene: {sceneName}");

            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);

            if (asyncOperation == null)
            {
                LogDebug($"Failed to start loading scene: {sceneName}");
                m_IsLoadingNow = false;
                yield break;
            }

            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.progress < 0.9f)
            {
                LogDebug($"Loading progress: {asyncOperation.progress * 100:F1}%");
                yield return null;
            }

            LogDebug("Scene loaded to 90%, ready to activate");
            m_IsLoadingComplete = true;

            yield return new WaitForSeconds(0.1f);

            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            m_IsLoadingNow = false;
            LogDebug($"Scene {sceneName} loaded successfully");
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[SceneController] {message}");
        }

        void OnDestroy()
        {
            LogDebug("SceneController destroyed");
        }

        [ContextMenu("Load Start Scene")]
        public void LoadStartScene()
        {
            LoadScene(StartSceneName);
        }

        [ContextMenu("Load Core Scene")]
        public void LoadCoreScene()
        {
            LoadScene(CoreSceneName);
        }
    }
}