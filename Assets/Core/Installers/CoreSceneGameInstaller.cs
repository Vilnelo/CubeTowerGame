using Core.Canvases.External;
using Core.UI.External;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    public class CoreSceneGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("CoreSceneGameInstaller: Installing core scene services...");
            
            Container.BindInterfacesTo<MainCanvas>()
                .FromComponentInHierarchy()
                .AsSingle();
            
            Container.BindInterfacesTo<CoreUIController>()
                .AsSingle()
                .NonLazy();
            
            /*Container.BindInterfacesTo<TrashHoleController>()
                .AsSingle();

            Container.BindInterfacesTo<TowerController>()
                .AsSingle();

            Container.BindInterfacesTo<BottomController>()
                .AsSingle();*/

            /*// Оркестратор игры (координирует все фичи)
            Container.BindInterfacesTo<GameOrchestrator>()
                .AsSingle();
                */

            Debug.Log("CoreSceneGameInstaller: Core scene services installed successfully");
        }
    }
}
