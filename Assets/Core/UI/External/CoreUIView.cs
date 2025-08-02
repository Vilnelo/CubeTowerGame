using Core.BottomBlocks.External;
using Core.Tower.External;
using Core.TrashHole.External;
using UnityEngine;

namespace Core.UI.External
{
    public class CoreUIView : MonoBehaviour
    {
        [Header("View References")] [SerializeField]
        private TrashHoleView m_TrashHoleView;

        [SerializeField] private TowerView m_TowerView;
        [SerializeField] private BottomBlocksView m_BottomBlocksView;
        [SerializeField] private Transform m_DraggingBlockView;

        public TrashHoleView TrashHoleView => m_TrashHoleView;
        public TowerView TowerView => m_TowerView;
        public BottomBlocksView BottomBlocksView => m_BottomBlocksView;
        public Transform DraggingBlockView => m_DraggingBlockView;
    }
}