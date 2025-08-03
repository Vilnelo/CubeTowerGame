using UnityEngine;

namespace Core.DragAndDrop.Runtime
{
    public interface IDraggable
    {
        DragType GetDragBehavior();
        DragType SetDragBehavior(DragType dragBehavior);
        void OnDragStart();
        void OnDragEnd(Vector3 endPosition);
        GameObject GetGameObject();

        void OnMoveStart();
        void OnMoveEnd();
    }
}