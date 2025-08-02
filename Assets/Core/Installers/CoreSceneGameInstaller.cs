using Core.BottomBlocks.External;
using Core.Canvases.External;
using Core.DragAndDrop.External;
using Core.InputSystem.External;
using Core.TrashHole.External;
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
            
            Container.BindInterfacesAndSelfTo<TrashHoleController>()
                .AsSingle();
            
            Container.BindInterfacesTo<LayoutUIController>()
                .AsSingle();
            
            Container.BindInterfacesTo<BottomBlocksController>()
                .AsSingle();
            
            Container.BindInterfacesTo<InputManager>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();
            
            Container.BindInterfacesTo<DragAndDropController>()
                .AsSingle()
                .NonLazy();

            Debug.Log("CoreSceneGameInstaller: Core scene services installed successfully");
        }
    }
}
