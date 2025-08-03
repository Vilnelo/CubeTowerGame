using UnityEngine;

namespace Core.UI.Runtime
{
    public interface ILayoutUIController
    {
        float GetCubeSize();
        float GetCubeWidth();
        float GetCubeHeight();
        Vector2 GetCubeSizeVector();
        int GetMaxCubesInTower();
        void OnScreenSizeChanged();
    }
}