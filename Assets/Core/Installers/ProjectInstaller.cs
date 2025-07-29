using Core.Scene.External;
using Core.Scene.Runtime;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            LogDebug("Installing global services...");
            
            InstallSceneController();

            LogDebug("Global services installed successfully");
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
        
        private void LogDebug(string message)
        {
            
                Debug.Log($"[ProjectInstaller] {message}");
           
        }
    }
}
