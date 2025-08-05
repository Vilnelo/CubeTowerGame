using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.DragAndDrop.Runtime
{
    public class DragState
    {
        public BlockView m_CurrentBlockView { get; set; }
        public BlockView m_OriginalBlockView { get; set; }
        public GameObject m_CurrentDraggedObject { get; set; }
        public bool m_IsWaitingForPickup { get; set; }
        public bool m_IsDragging { get; set; }
        public bool m_HasPickedUpBlock { get; set; }
        public bool m_IsCloneOperation { get; set; }

        public void Reset()
        {
            m_CurrentBlockView = null;
            m_OriginalBlockView = null;
            m_CurrentDraggedObject = null;
            m_IsWaitingForPickup = false;
            m_IsDragging = false;
            m_HasPickedUpBlock = false;
            m_IsCloneOperation = false;
        }
    }
}