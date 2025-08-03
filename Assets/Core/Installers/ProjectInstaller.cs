using Core.AssetManagement.External;
using Core.AssetManagement.Runtime;
using Core.ConfigSystem.External;
using Core.ConfigSystem.Runtime;
using Core.Localization.External;
using Core.Localization.Runtime;
using Core.SaveSystem.External;
using Core.Scene.External;
using Core.Scene.Runtime;
using UnityEngine;
using Utils.SimpleTimerSystem.External;
using Utils.SimpleTimerSystem.Runtime;
using Zenject;

namespace Core.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            LogDebug("Installing global services...");

            InstallSceneController();
            InstallAssetLoader();
            InstallLocalization();
            InstallConfigSystem();
            InstallSaveSystem();
            InstallTimerController();

            LogDebug("Global services installed successfully");
        }
        
        private void InstallLocalization()
        {
            Container.BindInterfacesTo<LocalizationController>()
                .AsSingle()
                .NonLazy();

            LogDebug("LocalizationController bound successfully");
        }
        
        private void InstallSaveSystem()
        {
            Container.BindInterfacesTo<AutoSaveController>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            LogDebug("AutoSave system bound successfully");
        }

        private void InstallSceneController()
        {
            Container.Bind<ISceneController>()
                .To<SceneController>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            LogDebug("SceneController bound successfully");
        }

        private void InstallAssetLoader()
        {
            Container.Bind<IAssetLoader>()
                .To<AssetLoader>()
                .AsSingle();

            LogDebug("AssetLoader bound successfully");
        }

        private void InstallTimerController()
        {
            Container.Bind<ITimerController>()
                .To<TimerController>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            LogDebug("TimerController bound successfully");
        }

        private void InstallConfigSystem()
        {
            Container.Bind<IConfigReader>()
                .To<ConfigReader>()
                .AsSingle();

            Container.Bind<IConfigLoader>()
                .To<ConfigLoader>()
                .AsSingle();

            LogDebug("ConfigSystem bound successfully");
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[ProjectInstaller] {message}");
        }
    }
}