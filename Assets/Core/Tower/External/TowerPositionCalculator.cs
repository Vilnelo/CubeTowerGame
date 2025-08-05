using Core.Animations.External.PopupText.External;
using Core.BottomBlocks.External;
using Core.Localization.Runtime;
using Core.Tower.Runtime;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerPositionCalculator : ITowerPositionCalculator
    {
        private readonly TowerAreaDetector m_AreaDetector;
        private readonly RectTransform m_TowerArea;

        public TowerPositionCalculator(TowerAreaDetector areaDetector, RectTransform towerArea)
        {
            m_AreaDetector = areaDetector;
            m_TowerArea = towerArea;
        }

        public bool TryCalculatePlacementPosition(BlockView blockView, Vector3 desiredPosition, TowerState towerState,
            out Vector3 finalPosition)
        {
            finalPosition = desiredPosition;

            if (towerState.ActiveBlocks.Count == 0)
            {
                finalPosition = desiredPosition;
                return true;
            }

            var topBlock = towerState.GetTopBlock();
            if (topBlock == null) return false;

            return TryFindPlacementAboveTopBlockWithOffset(blockView, desiredPosition, topBlock, towerState,
                out finalPosition);
        }

        private bool TryFindPlacementAboveTopBlockWithOffset(BlockView blockView, Vector3 desiredPosition,
            BlockView topBlock, TowerState towerState, out Vector3 placementPosition)
        {
            placementPosition = desiredPosition;

            var topBlockRect = topBlock.GetRectTransform();
            Vector3[] topBlockCorners = new Vector3[4];
            topBlockRect.GetWorldCorners(topBlockCorners);

            float topBlockMinX = topBlockCorners[0].x;
            float topBlockMaxX = topBlockCorners[2].x;
            float topBlockCenterX = (topBlockMinX + topBlockMaxX) / 2;
            float topBlockCenterY = (topBlockCorners[0].y + topBlockCorners[2].y) / 2;
            float highestY = topBlockCorners[2].y;

            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = desiredPosition;

            var blockRect = blockView.GetRectTransform();
            blockRect.ForceUpdateRectTransforms();

            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);

            float blockHeight = blockCorners[2].y - blockCorners[0].y;
            float blockMinX = blockCorners[0].x;
            float blockMaxX = blockCorners[2].x;
            float blockCenterY = (blockCorners[0].y + blockCorners[2].y) / 2;

            blockView.transform.position = originalPosition;

            bool hasXOverlapWithAnyBlock = false;
            foreach (var existingBlock in towerState.ActiveBlocks)
            {
                if (existingBlock == null) continue;

                var existingRect = existingBlock.GetRectTransform();
                Vector3[] existingCorners = new Vector3[4];
                existingRect.GetWorldCorners(existingCorners);

                float existingMinX = existingCorners[0].x;
                float existingMaxX = existingCorners[2].x;

                bool overlapWithThis = (blockMinX <= existingMaxX && blockMaxX >= existingMinX);
                if (overlapWithThis)
                {
                    hasXOverlapWithAnyBlock = true;
                    Debug.Log($"TowerPositionCalculator: Block overlaps with {existingBlock.GetColorName()} by X");
                    break;
                }
            }

            bool isAboveTopBlockCenter = blockCenterY > topBlockCenterY;

            Debug.Log(
                $"TowerPositionCalculator: Block center Y: {blockCenterY:F2}, Top block center Y: {topBlockCenterY:F2}");
            Debug.Log(
                $"TowerPositionCalculator: X overlap with any block: {hasXOverlapWithAnyBlock}, Above top center: {isAboveTopBlockCenter}");

            if (!hasXOverlapWithAnyBlock)
            {
                Debug.LogWarning(
                    $"TowerPositionCalculator: Block {blockView.GetColorName()} does not overlap with any block in tower by X coordinate");
                return false;
            }

            if (!isAboveTopBlockCenter)
            {
                Debug.LogWarning(
                    $"TowerPositionCalculator: Block {blockView.GetColorName()} is not above the center of top block {topBlock.GetColorName()}");
                return false;
            }

            float randomOffset = CalculateRandomOffsetWithLimits(topBlock, towerState);

            Vector3 basePosition = new Vector3(topBlockCenterX + randomOffset, highestY + blockHeight / 2,
                desiredPosition.z);

            Debug.Log(
                $"TowerPositionCalculator: Calculated placement position with offset {randomOffset}: {basePosition}");

            if (!IsBlockCompletelyInTowerArea(blockView, basePosition))
            {
                if (IsBlockExceedingTopBoundary(blockView, basePosition))
                {
                    Debug.LogWarning("TowerPositionCalculator: Block exceeds top boundary - will be destroyed");
                    PopupTextController.ShowPopupText("NoMoreSpaceText".Localize());
                    return false;
                }

                Debug.LogWarning("TowerPositionCalculator: Block outside side boundaries, trying without offset");

                basePosition = new Vector3(topBlockCenterX, highestY + blockHeight / 2, desiredPosition.z);
                if (!IsBlockCompletelyInTowerArea(blockView, basePosition))
                {
                    Debug.LogWarning(
                        "TowerPositionCalculator: Block still outside area even without offset - will be destroyed");
                    PopupTextController.ShowPopupText("NoMoreSpaceText".Localize());
                    return false;
                }
            }

            placementPosition = basePosition;
            return true;
        }

        private float CalculateRandomOffsetWithLimits(BlockView topBlock, TowerState towerState)
        {
            var topRect = topBlock.GetRectTransform();
            Vector3[] topCorners = new Vector3[4];
            topRect.GetWorldCorners(topCorners);

            float topLeftX = topCorners[0].x;
            float topRightX = topCorners[2].x;
            float topWidth = topRightX - topLeftX;
            float topCenterX = (topLeftX + topRightX) / 2f;

            if (towerState.ActiveBlocks.Count == 0)
            {
                Debug.LogWarning("TowerPositionCalculator: No blocks in tower for offset calculation");
                return 0f;
            }

            BlockView baseBlock = towerState.ActiveBlocks[0];
            var baseRect = baseBlock.GetRectTransform();
            Vector3[] baseCorners = new Vector3[4];
            baseRect.GetWorldCorners(baseCorners);

            float baseLeftX = baseCorners[0].x;
            float baseRightX = baseCorners[2].x;
            float baseWidth = baseRightX - baseLeftX;
            float baseCenterX = (baseLeftX + baseRightX) / 2f;

            float topRangeLeft = topCenterX - (topWidth * 0.5f);
            float topRangeRight = topCenterX + (topWidth * 0.5f);

            float baseRangeLeft = baseCenterX - (baseWidth * 0.5f);
            float baseRangeRight = baseCenterX + (baseWidth * 0.5f);

            float intersectionLeft = Mathf.Max(topRangeLeft, baseRangeLeft);
            float intersectionRight = Mathf.Min(topRangeRight, baseRangeRight);

            if (intersectionLeft >= intersectionRight)
            {
                Debug.LogWarning("TowerPositionCalculator: No intersection found. Using 0 offset.");
                return 0f;
            }

            float chosenCenterX = Random.Range(intersectionLeft, intersectionRight);
            float offsetFromTop = chosenCenterX - topCenterX;

            Debug.Log(
                $"TowerPositionCalculator: Base block center: {baseCenterX:F2}, Top block center: {topCenterX:F2}, Chosen offset: {offsetFromTop:F2}");
            return offsetFromTop;
        }

        private bool IsBlockCompletelyInTowerArea(BlockView blockView, Vector3 position)
        {
            if (m_AreaDetector == null || blockView == null)
            {
                Debug.LogWarning("TowerPositionCalculator: AreaDetector or BlockView is null");
                return false;
            }

            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = position;

            bool isInside = m_AreaDetector.IsBlockCompletelyInside(blockView.GetRectTransform());

            blockView.transform.position = originalPosition;

            Debug.Log($"TowerPositionCalculator: Block at {position} - Completely inside: {isInside}");
            return isInside;
        }

        private bool IsBlockExceedingTopBoundary(BlockView blockView, Vector3 position)
        {
            if (m_TowerArea == null || blockView == null)
            {
                return false;
            }

            Vector3[] towerCorners = new Vector3[4];
            m_TowerArea.GetWorldCorners(towerCorners);
            float towerTop = towerCorners[2].y;

            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = position;

            var blockRect = blockView.GetRectTransform();
            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);
            float blockTop = blockCorners[2].y;

            blockView.transform.position = originalPosition;

            bool exceedsTop = blockTop > towerTop;
            Debug.Log(
                $"TowerPositionCalculator: Block top: {blockTop:F2}, Tower top: {towerTop:F2}, Exceeds: {exceedsTop}");

            return exceedsTop;
        }

        public Vector3 CalculateNewPositionForBlock(BlockView blockView, float baseY)
        {
            var blockRect = blockView.GetRectTransform();
            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);

            float blockHeight = blockCorners[2].y - blockCorners[0].y;
            Vector3 currentPosition = blockView.transform.position;

            return new Vector3(currentPosition.x, baseY + blockHeight / 2, currentPosition.z);
        }

        public float GetBaseYForIndex(int index, TowerState towerState)
        {
            if (index == 0)
            {
                Vector3[] towerCorners = new Vector3[4];
                m_TowerArea.GetWorldCorners(towerCorners);
                float towerBottomY = towerCorners[0].y;
                Debug.Log($"TowerPositionCalculator: Base Y for first block: {towerBottomY}");
                return towerBottomY;
            }

            var previousBlock = towerState.ActiveBlocks[index - 1];
            var previousRect = previousBlock.GetRectTransform();

            Vector3[] previousCorners = new Vector3[4];
            previousRect.GetWorldCorners(previousCorners);

            float previousTopY = previousCorners[2].y;
            Debug.Log($"TowerPositionCalculator: Base Y from previous block: {previousTopY}");
            return previousTopY;
        }
    }
}