namespace Core.Scene.Runtime
{
    public interface ISceneController
    {
        string ActiveScene { get; }
        bool IsLoadingNow { get; }
        bool IsLoadingComplete { get; }
        void LoadScene(string sceneName);
        void Reload();
    }
}