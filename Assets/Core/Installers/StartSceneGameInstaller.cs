using Common.Canvases.External;
using Core.Scene.External;
using Zenject;

namespace Core.Installers
{
    public class StartSceneGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<MainCanvas>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container.BindInterfacesTo<StartSceneController>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}