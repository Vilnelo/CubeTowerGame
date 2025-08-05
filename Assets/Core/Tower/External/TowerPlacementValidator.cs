using Core.BottomBlocks.External;
using Core.Tower.Runtime;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerPlacementValidator : ITowerPlacementValidator
    {
        private readonly TowerAreaDetector m_AreaDetector;

        public TowerPlacementValidator(TowerAreaDetector areaDetector)
        {
            m_AreaDetector = areaDetector;
        }

        public bool CanPlaceBlock(BlockView blockView, Vector3 position, TowerState towerState)
        {
            if (!ValidatePlacementConditions(blockView, position, towerState))
            {
                return false;
            }

            if (towerState.ActiveBlocks.Count == 0)
            {
                bool canPlace = IsBlockInTowerArea(blockView, position);
                Debug.Log($"TowerPlacementValidator: First block can be placed: {canPlace}");
                return canPlace;
            }

            return ValidateStackPlacement(blockView, position, towerState);
        }

        public bool ValidatePlacementConditions(BlockView blockView, Vector3 position, TowerState towerState)
        {
            if (blockView == null)
            {
                Debug.LogError("TowerPlacementValidator: BlockView is null");
                return false;
            }

            if (towerState.IsCollapseAnimationPlaying)
            {
                Debug.Log("TowerPlacementValidator: Cannot place - collapse animation playing");
                return false;
            }

            return true;
        }

        private bool IsBlockInTowerArea(BlockView blockView, Vector3 position)
        {
            if (m_AreaDetector == null || blockView == null)
            {
                Debug.LogWarning("TowerPlacementValidator: AreaDetector or BlockView is null");
                return false;
            }

            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = position;

            bool isInside = m_AreaDetector.IsBlockCompletelyInside(blockView.GetRectTransform());

            blockView.transform.position = originalPosition;

            Debug.Log($"TowerPlacementValidator: Block at {position} - Inside area: {isInside}");
            return isInside;
        }

        private bool ValidateStackPlacement(BlockView blockView, Vector3 position, TowerState towerState)
        {
            var topBlock = towerState.GetTopBlock();
            if (topBlock == null)
            {
                Debug.LogError("TowerPlacementValidator: No top block found");
                return false;
            }

            return ValidateBlockAboveTopBlock(blockView, position, topBlock, towerState);
        }

        private bool ValidateBlockAboveTopBlock(BlockView blockView, Vector3 position, BlockView topBlock,
            TowerState towerState)
        {
            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = position;

            var blockRect = blockView.GetRectTransform();
            blockRect.ForceUpdateRectTransforms();

            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);

            float blockMinX = blockCorners[0].x;
            float blockMaxX = blockCorners[2].x;
            float blockCenterY = (blockCorners[0].y + blockCorners[2].y) / 2;

            blockView.transform.position = originalPosition;

            bool hasXOverlap = false;
            foreach (var existingBlock in towerState.ActiveBlocks)
            {
                if (existingBlock == null) continue;

                var existingRect = existingBlock.GetRectTransform();
                Vector3[] existingCorners = new Vector3[4];
                existingRect.GetWorldCorners(existingCorners);

                float existingMinX = existingCorners[0].x;
                float existingMaxX = existingCorners[2].x;

                if (blockMinX <= existingMaxX && blockMaxX >= existingMinX)
                {
                    hasXOverlap = true;
                    break;
                }
            }

            if (!hasXOverlap)
            {
                Debug.LogWarning("TowerPlacementValidator: No X overlap with tower blocks");
                return false;
            }

            var topRect = topBlock.GetRectTransform();
            Vector3[] topCorners = new Vector3[4];
            topRect.GetWorldCorners(topCorners);
            float topCenterY = (topCorners[0].y + topCorners[2].y) / 2;

            if (blockCenterY <= topCenterY)
            {
                Debug.LogWarning("TowerPlacementValidator: Block not above top block center");
                return false;
            }

            Debug.Log("TowerPlacementValidator: Block can be placed above top block");
            return true;
        }
    }
}