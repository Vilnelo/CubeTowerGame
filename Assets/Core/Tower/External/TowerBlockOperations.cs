using System.Collections.Generic;
using System.Linq;
using Core.BottomBlocks.External;
using Core.DragAndDrop.Runtime;
using Core.Tower.Runtime;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerBlockOperations : ITowerBlockOperations
    {
        private readonly ITowerPlacementValidator m_PlacementValidator;
        private readonly ITowerPositionCalculator m_PositionCalculator;
        private readonly ITowerAnimationManager m_AnimationManager;
        private readonly ITowerSaveManager m_SaveManager;
        private readonly RectTransform m_TowerArea;

        public TowerBlockOperations(
            ITowerPlacementValidator placementValidator,
            ITowerPositionCalculator positionCalculator,
            ITowerAnimationManager animationManager,
            ITowerSaveManager saveManager,
            RectTransform towerArea)
        {
            m_PlacementValidator = placementValidator;
            m_PositionCalculator = positionCalculator;
            m_AnimationManager = animationManager;
            m_SaveManager = saveManager;
            m_TowerArea = towerArea;
        }

        public bool PlaceBlock(BlockView blockView, Vector3 position, TowerState towerState)
        {
            if (!m_PlacementValidator.CanPlaceBlock(blockView, position, towerState))
            {
                Debug.LogWarning("TowerBlockOperations: Cannot place block - validation failed");
                return false;
            }

            if (towerState.ActiveBlocks.Count == 0)
            {
                PlaceBlockDirectly(blockView, position, towerState);
                return true;
            }
            else
            {
                if (m_PositionCalculator.TryCalculatePlacementPosition(blockView, position, towerState,
                        out Vector3 finalPosition))
                {
                    PlaceBlockWithAnimation(blockView, finalPosition, towerState);
                    return true;
                }
                else
                {
                    Debug.LogWarning("TowerBlockOperations: Could not calculate placement position");
                    return false;
                }
            }
        }

        public void RemoveBlock(BlockView blockView, TowerState towerState)
        {
            if (blockView == null)
            {
                Debug.LogWarning("TowerBlockOperations: Attempted to remove null block");
                return;
            }

            if (towerState.IsCollapseAnimationPlaying)
            {
                Debug.Log("TowerBlockOperations: Cannot remove - collapse animation playing");
                return;
            }

            int removedIndex = towerState.GetBlockIndex(blockView);

            if (removedIndex == -1)
            {
                Debug.LogWarning($"TowerBlockOperations: Block {blockView.GetColorName()} not found");
                towerState.CleanupNullBlocks();
                m_SaveManager.SaveTowerState(towerState);
                return;
            }

            towerState.RemoveBlock(blockView);

            if (removedIndex < towerState.ActiveBlocks.Count)
            {
                CollapseBlocksAboveIndex(removedIndex, towerState);
            }
            else
            {
                m_SaveManager.SaveTowerState(towerState);
                Debug.Log("TowerBlockOperations: Removed last block from tower");
            }
        }

        private void PlaceBlockDirectly(BlockView blockView, Vector3 position, TowerState towerState)
        {
            SetupBlockAtPosition(blockView, position);
            towerState.AddBlock(blockView);
            m_SaveManager.SaveTowerState(towerState);

            Debug.Log($"TowerBlockOperations: Placed block {blockView.GetColorName()} directly");
        }

        private void PlaceBlockWithAnimation(BlockView blockView, Vector3 targetPosition, TowerState towerState)
        {
            Debug.Log($"TowerBlockOperations: Placing block {blockView.GetColorName()} with animation");

            m_AnimationManager.StartJumpAnimation(blockView, targetPosition,
                () => { OnBlockAnimationComplete(blockView, targetPosition, towerState); });
        }

        private void OnBlockAnimationComplete(BlockView blockView, Vector3 finalPosition, TowerState towerState)
        {
            SetupBlockAtPosition(blockView, finalPosition);
            towerState.AddBlock(blockView);
            m_SaveManager.SaveTowerState(towerState);

            Debug.Log($"TowerBlockOperations: Animation completed for {blockView.GetColorName()}");
        }

        private void SetupBlockAtPosition(BlockView blockView, Vector3 position)
        {
            blockView.transform.position = position;

            var blockRect = blockView.GetRectTransform();
            blockRect?.ForceUpdateRectTransforms();

            blockView.transform.SetAsLastSibling();
            blockView.GetDraggableBlockController().SetDragType(DragType.Move);

            Canvas.ForceUpdateCanvases();
        }

        private void CollapseBlocksAboveIndex(int startIndex, TowerState towerState)
        {
            Debug.Log($"TowerBlockOperations: Starting collapse from index {startIndex}");

            var blocksToAnimate = new List<GameObject>();
            var targetPositions = new List<Vector3>();

            float baseY = m_PositionCalculator.GetBaseYForIndex(startIndex, towerState);

            for (int i = startIndex; i < towerState.ActiveBlocks.Count; i++)
            {
                var blockToMove = towerState.ActiveBlocks[i];
                if (blockToMove == null) continue;

                blocksToAnimate.Add(blockToMove.gameObject);

                Vector3 newPosition = m_PositionCalculator.CalculateNewPositionForBlock(blockToMove, baseY);
                targetPositions.Add(newPosition);

                var rect = blockToMove.GetRectTransform();
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                float blockHeight = corners[2].y - corners[0].y;
                baseY = newPosition.y + blockHeight / 2;
            }

            if (blocksToAnimate.Count > 0)
            {
                m_AnimationManager.StartCollapseAnimation(blocksToAnimate, targetPositions,
                    () => { OnCollapseComplete(towerState); });
            }
            else
            {
                OnCollapseComplete(towerState);
            }
        }

        private void OnCollapseComplete(TowerState towerState)
        {
            foreach (var block in towerState.ActiveBlocks.Where(b => b != null))
            {
                var rect = block.GetRectTransform();
                rect?.ForceUpdateRectTransforms();
            }

            Canvas.ForceUpdateCanvases();
            m_SaveManager.SaveTowerState(towerState);

            Debug.Log("TowerBlockOperations: Collapse animation completed");
        }
    }
}