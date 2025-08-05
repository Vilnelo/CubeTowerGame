using Core.BottomBlocks.External;
using Core.DragAndDrop.Runtime;
using Core.Tower.Runtime;
using Core.TrashHole.Runtime;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class DragResultResolver : IDragResultResolver
    {
        private readonly ITrashHoleController m_TrashHoleController;
        private readonly ITowerController m_TowerController;

        public DragResultResolver(ITrashHoleController trashHoleController, ITowerController towerController)
        {
            m_TrashHoleController = trashHoleController;
            m_TowerController = towerController;
        }

        public DragResult ResolveDragResult(BlockView blockView, GameObject draggedObject, Vector3 endPosition)
        {
            var result = new DragResult
            {
                BlockView = blockView,
                DraggedObject = draggedObject,
                Position = endPosition
            };

            var blockRect = blockView.GetRectTransform();
        
            if (blockRect != null && m_TrashHoleController.IsBlockTouchingHole(blockRect))
            {
                result.ResultType = DragResultType.TrashDestruction;
                Debug.Log("DragResultResolver: Block touching trash hole - will be destroyed");
                return result;
            }

            var correctedEndPosition = new Vector3(endPosition.x, endPosition.y, 0f);
            if (m_TowerController.CanPlaceBlockInTower(blockView, correctedEndPosition))
            {
                result.ResultType = DragResultType.TowerPlacement;
                result.Position = correctedEndPosition;
                Debug.Log("DragResultResolver: Block can be placed in tower");
                return result;
            }

            result.ResultType = DragResultType.RegularDestruction;
            Debug.Log("DragResultResolver: Block will be destroyed - no valid placement");
            return result;
        }
    }
}