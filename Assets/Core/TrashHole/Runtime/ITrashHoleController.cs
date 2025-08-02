using UnityEngine;

namespace Core.TrashHole.Runtime
{
    public interface ITrashHoleController
    {
        bool IsBlockTouchingHole(RectTransform blockRect);
        Vector3 GetHoleCenterWorldPosition();
        void DestroyBlockInHole(GameObject block);
    }
}