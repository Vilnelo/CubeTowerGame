using Core.DragAndDrop.Runtime;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class DraggableBlockController : MonoBehaviour, IDraggable
    {
        private DragType m_DragBehavior;
        private Vector3 m_OriginalPosition;
        private Transform m_OriginalParent;

        public void Init()
        {
        }

        public DragType GetDragBehavior()
        {
            return m_DragBehavior;
        }

        public DragType SetDragBehavior(DragType dragBehavior)
        {
            return m_DragBehavior = dragBehavior;
        }

        public void SetDragType(DragType behavior)
        {
            m_DragBehavior = behavior;
            Debug.Log($"DraggableBlock: Set drag behavior to {behavior}");
        }

        public void OnDragStart()
        {
            if (m_DragBehavior == DragType.Clone)
            {
                Debug.Log($"DraggableBlock: Started cloning block");
            }
            else
            {
                OnMoveStart();
            }
        }

        public void OnDragEnd(Vector3 endPosition)
        {
            if (m_DragBehavior == DragType.Clone)
            {
                m_DragBehavior = DragType.Move;
            }
            else
            {
                OnMoveEnd();
            }
        }

        public void OnMoveStart()
        {
            Debug.Log($"DraggableBlock: Started moving block");

            m_OriginalPosition = transform.position;
            m_OriginalParent = transform.parent;
        }

        public void OnMoveEnd()
        {
            Debug.Log($"DraggableBlock: Finished moving block");
            ReturnToOriginalPosition();
        }

        public void ReturnToOriginalPosition()
        {
            if (m_DragBehavior == DragType.Move)
            {
                transform.position = m_OriginalPosition;
                transform.SetParent(m_OriginalParent);
                Debug.Log($"DraggableBlock: Returned block to original position");
            }
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}