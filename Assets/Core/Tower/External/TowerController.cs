using System.Collections.Generic;
using System.Linq;
using Core.Animations.External;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime;
using Core.Canvases.Runtime;
using Core.DragAndDrop.Runtime;
using Core.SaveSystem.Runtime;
using Core.Tower.Runtime;
using Core.Tower.Runtime.Data;
using Core.UI.Runtime;
using UnityEngine;
using Zenject;

namespace Core.Tower.External
{
    public class TowerController : ITowerController, IInitializable
    {
        [Inject] private ICoreUIController m_CoreUIController;
        [Inject] private IAutoSaveController m_SaveController;
        [Inject] private IBlockFactoryController m_BlockFactoryController;
        [Inject] private IMainCanvas m_MainCanvas;

        private TowerSaveData m_SaveData = new TowerSaveData();
        private TowerAreaDetector m_AreaDetector;
        private readonly List<BlockView> m_ActiveBlocks = new List<BlockView>();
        private TowerJumpAnimation m_JumpAnimation;
        private TowerCollapseAnimation m_CollapseAnimation;

        public void Initialize()
        {
            Debug.Log("TowerController: Initializing...");

            var coreView = m_CoreUIController.GetCoreView();
            if (coreView?.TowerView?.Tower != null)
            {
                m_AreaDetector = new TowerAreaDetector(coreView.TowerView.Tower);
                Debug.Log("TowerController: Tower area detector created");
            }
            else
            {
                Debug.LogError("TowerController: Tower area not found!");
                return;
            }
            
            m_JumpAnimation = new TowerJumpAnimation();
            m_CollapseAnimation = new TowerCollapseAnimation();
            m_SaveController.RegisterAutoSave(m_SaveData);
            
            LoadTowerState();

            Debug.Log("TowerController: Initialized successfully");
        }

        public bool IsBlockInTowerArea(BlockView blockView, Vector3 position)
        {
            if (m_AreaDetector == null || blockView == null)
            {
                Debug.LogWarning("TowerController: AreaDetector or BlockView is null");
                return false;
            }
            
            var coreView = m_CoreUIController.GetCoreView();
            Transform towerParent = coreView.TowerView.Tower;

            Debug.Log($"TowerController: Checking position {position} against tower area {towerParent.name}");
            
            Vector3 originalPosition = blockView.transform.position;
            
            blockView.transform.position = position;
            
            bool isCompletelyInside = m_AreaDetector.IsBlockCompletelyInside(blockView.GetRectTransform());
            
            blockView.transform.position = originalPosition;

            Debug.Log($"TowerController: Block {blockView.GetColorName()} at {position} - " +
                      $"Completely inside: {isCompletelyInside}");

            return isCompletelyInside;
        }

        public bool CanPlaceBlockInTower(BlockView blockView, Vector3 position)
        {
            if (blockView == null)
            {
                Debug.LogError("TowerController: BlockView is null");
                return false;
            }
            
            if (m_ActiveBlocks.Count == 0)
            {
                bool canPlace = IsBlockInTowerArea(blockView, position);
                Debug.Log($"TowerController: First block can be placed: {canPlace}");
                return canPlace;
            }
            else
            {
                Vector3 finalPosition;
                bool canPlace = TryFindPlacementAboveTopBlock(blockView, position, out finalPosition);
                Debug.Log($"TowerController: Block can be placed above top block: {canPlace}");
                return canPlace;
            }
        }

        public bool TryPlaceBlockInTower(BlockView blockView, Vector3 position)
        {
            if (blockView == null)
            {
                Debug.LogError("TowerController: BlockView is null");
                return false;
            }
            
            if (!CanPlaceBlockInTower(blockView, position))
            {
                Debug.Log($"TowerController: Block {blockView.GetColorName()} cannot be placed at this position");
                return false;
            }

            Vector3 finalPosition = position;
            
            if (m_ActiveBlocks.Count == 0)
            {
                Debug.Log($"TowerController: Placing first block at position {position}");
                PlaceBlockWithAnimation(blockView, position);
                return true;
            }
            else
            {
                if (TryFindPlacementAboveTopBlock(blockView, position, out finalPosition))
                {
                    Debug.Log($"TowerController: Placing block above top block at position {finalPosition}");
                    PlaceBlockWithAnimation(blockView, finalPosition);
                    return true;
                }
                else
                {
                    Debug.LogError("TowerController: Unexpected error - CanPlaceBlockInTower returned true but TryFindPlacementAboveTopBlock failed");
                    return false;
                }
            }
        }

        private void PlaceBlockWithAnimation(BlockView blockView, Vector3 targetPosition)
        {
            Canvas canvas = m_MainCanvas.GetCanvas();
            
            if (canvas == null)
            {
                Debug.LogWarning("TowerController: Main canvas not found, placing block without animation");
                PlaceBlockDirectly(blockView, targetPosition);
                return;
            }

            Debug.Log($"TowerController: Starting jump animation for {blockView.GetColorName()} to {targetPosition}");
            
            m_JumpAnimation.StartJumpAnimation(blockView.gameObject, targetPosition, canvas, () =>
            {
                OnBlockAnimationComplete(blockView, targetPosition);
            });
        }

        private void OnBlockAnimationComplete(BlockView blockView, Vector3 finalPosition)
        {
            Debug.Log($"TowerController: Animation completed for {blockView.GetColorName()}");
            
            blockView.transform.position = finalPosition;
            
            RectTransform blockRect = blockView.GetRectTransform();
            if (blockRect != null)
            {
                blockRect.ForceUpdateRectTransforms();
            }
            
            blockView.transform.SetAsLastSibling();
            blockView.GetDraggableBlockController().SetDragType(DragType.Move);
            
            if (!m_ActiveBlocks.Contains(blockView))
            {
                m_ActiveBlocks.Add(blockView);
                Debug.Log($"TowerController: Added block to active blocks. Total count: {m_ActiveBlocks.Count}");
            }
            
            SaveCurrentTowerState();
            Canvas.ForceUpdateCanvases();

            Debug.Log($"TowerController: Successfully placed block {blockView.GetColorName()} at {finalPosition}");
        }

        private void PlaceBlockDirectly(BlockView blockView, Vector3 targetPosition)
        {
            blockView.transform.position = targetPosition;
            
            RectTransform blockRect = blockView.GetRectTransform();
            if (blockRect != null)
            {
                blockRect.ForceUpdateRectTransforms();
            }
            
            blockView.transform.SetAsLastSibling();
            blockView.GetDraggableBlockController().SetDragType(DragType.Move);
            
            if (!m_ActiveBlocks.Contains(blockView))
            {
                m_ActiveBlocks.Add(blockView);
                Debug.Log($"TowerController: Added block to active blocks. Total count: {m_ActiveBlocks.Count}");
            }
            
            SaveCurrentTowerState();
            Canvas.ForceUpdateCanvases();

            Debug.Log($"TowerController: Successfully placed block {blockView.GetColorName()} at {targetPosition}");
        }

        private bool TryFindPlacementAboveTopBlock(BlockView blockView, Vector3 desiredPosition, out Vector3 placementPosition)
        {
            placementPosition = desiredPosition;
            
            BlockView topBlock = null;
            float highestY = float.MinValue;
            
            foreach (var existingBlock in m_ActiveBlocks.Where(block => block != null))
            {
                RectTransform existingRect = existingBlock.GetRectTransform();
                Vector3[] existingCorners = new Vector3[4];
                existingRect.GetWorldCorners(existingCorners);
                
                float existingTopY = existingCorners[2].y;
                
                if (existingTopY > highestY)
                {
                    highestY = existingTopY;
                    topBlock = existingBlock;
                }
            }
            
            if (topBlock == null)
            {
                Debug.LogError("TowerController: No top block found, but active blocks list is not empty");
                return false;
            }
            
            Debug.Log($"TowerController: Found top block {topBlock.GetColorName()} at Y: {highestY}");
            
            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = desiredPosition;
            
            RectTransform blockRect = blockView.GetRectTransform();
            blockRect.ForceUpdateRectTransforms();
            
            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);
            
            float blockHeight = blockCorners[2].y - blockCorners[0].y;
            
            blockView.transform.position = originalPosition;
            
            RectTransform topBlockRect = topBlock.GetRectTransform();
            Vector3[] topBlockCorners = new Vector3[4];
            topBlockRect.GetWorldCorners(topBlockCorners);
            
            float topBlockMinX = topBlockCorners[0].x;
            float topBlockMaxX = topBlockCorners[2].x;
            float blockMinX = blockCorners[0].x;
            float blockMaxX = blockCorners[2].x;
            
            bool hasXOverlap = (blockMinX <= topBlockMaxX && blockMaxX >= topBlockMinX);
            
            if (!hasXOverlap)
            {
                Debug.LogWarning($"TowerController: Block {blockView.GetColorName()} does not overlap with top block {topBlock.GetColorName()} by X coordinate");
                return false;
            }
            
            placementPosition = new Vector3(desiredPosition.x, highestY + blockHeight / 2, desiredPosition.z);
            
            Debug.Log($"TowerController: Calculated placement position above top block {topBlock.GetColorName()}: {placementPosition}");
            
            return IsBlockInTowerArea(blockView, placementPosition);
        }

        private void SaveCurrentTowerState()
        {
            m_SaveData.TowerBlocks.Clear();
            
            var validBlocks = m_ActiveBlocks.Where(block => block != null).ToList();
            m_ActiveBlocks.Clear();
            m_ActiveBlocks.AddRange(validBlocks);
            
            for (int i = 0; i < m_ActiveBlocks.Count; i++)
            {
                var blockView = m_ActiveBlocks[i];
                
                var blockData = new TowerBlockData(
                    blockView.GetId(),
                    blockView.transform.position,
                    i
                );
                
                m_SaveData.TowerBlocks.Add(blockData);
                Debug.Log($"TowerController: Saved block {blockView.GetColorName()} (layer {i}) at position {blockView.transform.position}");
            }

            Debug.Log($"TowerController: Saved {m_SaveData.TowerBlocks.Count} blocks to save data");
        }

        public void SaveTowerState()
        {
            SaveCurrentTowerState();
            m_SaveController.SaveData(m_SaveData);
            Debug.Log($"TowerController: Manually saved tower state with {m_SaveData.TowerBlocks.Count} blocks");
        }

        public void LoadTowerState()
        {
            Debug.Log($"TowerController: Loading tower state with {m_SaveData.TowerBlocks.Count} blocks");
            
            ClearCurrentBlocks();
            
            var coreView = m_CoreUIController.GetCoreView();
            var towerParent = coreView.DraggingBlockView;

            foreach (var blockData in m_SaveData.TowerBlocks.OrderBy(b => b.LayerIndex))
            {
                var blockView = m_BlockFactoryController.CreateBlockById(blockData.BlockId, towerParent, DragType.Move);

                if (blockView != null)
                {
                    Vector3 savedWorldPosition = blockData.GetPosition();
                    
                    blockView.transform.position = savedWorldPosition;
                    
                    RectTransform blockRect = blockView.GetRectTransform();
                    if (blockRect != null)
                    {
                        blockRect.ForceUpdateRectTransforms();
                    }
                    
                    m_ActiveBlocks.Add(blockView);

                    Debug.Log($"TowerController: Restored block Config ID {blockData.BlockId} (layer {blockData.LayerIndex}) at {savedWorldPosition}");
                }
                else
                {
                    Debug.LogError($"TowerController: Failed to create block with Config ID {blockData.BlockId}");
                }
            }
            
            Canvas.ForceUpdateCanvases();
            Debug.Log($"TowerController: Successfully loaded {m_ActiveBlocks.Count} blocks");
        }

        private void ClearCurrentBlocks()
        {
            foreach (var blockView in m_ActiveBlocks.Where(block => block != null))
            {
                Object.Destroy(blockView.gameObject);
            }

            m_ActiveBlocks.Clear();
            Debug.Log("TowerController: Cleared all current blocks");
        }

        public void RemoveBlockFromTower(BlockView blockView)
        {
            if (blockView == null)
            {
                Debug.LogWarning("TowerController: Attempted to remove null block from tower");
                return;
            }
            
            int removedBlockIndex = m_ActiveBlocks.IndexOf(blockView);
            
            if (removedBlockIndex == -1)
            {
                Debug.LogWarning($"TowerController: Block {blockView.GetColorName()} was not found in active blocks list");
                
                int removedNulls = m_ActiveBlocks.RemoveAll(block => block == null);
                if (removedNulls > 0)
                {
                    Debug.Log($"TowerController: Cleaned up {removedNulls} null blocks from active list");
                    SaveCurrentTowerState();
                }
                return;
            }

            Debug.Log($"TowerController: Removing block {blockView.GetColorName()} at index {removedBlockIndex}");
            
            m_ActiveBlocks.RemoveAt(removedBlockIndex);
            
            if (removedBlockIndex < m_ActiveBlocks.Count)
            {
                CollapseBlocksWithAnimation(removedBlockIndex);
            }
            else
            {
                SaveCurrentTowerState();
                Debug.Log($"TowerController: Removed last block from tower. Remaining blocks: {m_ActiveBlocks.Count}");
            }
        }

        private void CollapseBlocksWithAnimation(int startIndex)
        {
            Debug.Log($"TowerController: Starting animated collapse from index {startIndex}");
            
            List<GameObject> blocksToAnimate = new List<GameObject>();
            List<Vector3> targetPositions = new List<Vector3>();

            float baseY = GetBaseYForIndex(startIndex);

            for (int i = startIndex; i < m_ActiveBlocks.Count; i++)
            {
                var blockToMove = m_ActiveBlocks[i];
                
                if (blockToMove == null) continue;

                blocksToAnimate.Add(blockToMove.gameObject);
                
                Vector3 newPosition = CalculateNewPositionForBlock(blockToMove, baseY);
                targetPositions.Add(newPosition);
                
                Debug.Log($"TowerController: Block {blockToMove.GetColorName()} will move to {newPosition} (new layer {i})");
                
                RectTransform blockRect = blockToMove.GetRectTransform();
                Vector3[] corners = new Vector3[4];
                blockRect.GetWorldCorners(corners);
                float blockHeight = corners[2].y - corners[0].y;
                baseY = newPosition.y + blockHeight / 2;
            }
            
            if (blocksToAnimate.Count > 0)
            {
                m_CollapseAnimation.StartCollapseAnimation(blocksToAnimate, targetPositions, () =>
                {
                    OnCollapseAnimationComplete();
                });
            }
            else
            {
                OnCollapseAnimationComplete();
            }
        }

        private void OnCollapseAnimationComplete()
        {
            Debug.Log("TowerController: Collapse animation completed");
            
            foreach (var block in m_ActiveBlocks.Where(b => b != null))
            {
                RectTransform blockRect = block.GetRectTransform();
                if (blockRect != null)
                {
                    blockRect.ForceUpdateRectTransforms();
                }
            }

            Canvas.ForceUpdateCanvases();
            SaveCurrentTowerState();
            
            Debug.Log($"TowerController: Completed animated collapse. Remaining blocks: {m_ActiveBlocks.Count}");
        }

        private float GetBaseYForIndex(int index)
        {
            if (index == 0)
            {
                var coreView = m_CoreUIController.GetCoreView();
                RectTransform towerRect = coreView.TowerView.Tower;
                
                Vector3[] towerCorners = new Vector3[4];
                towerRect.GetWorldCorners(towerCorners);
                
                float towerBottomY = towerCorners[0].y;
                Debug.Log($"TowerController: Base Y for first block: {towerBottomY}");
                return towerBottomY;
            }
            
            var previousBlock = m_ActiveBlocks[index - 1];
            RectTransform previousRect = previousBlock.GetRectTransform();
            
            Vector3[] previousCorners = new Vector3[4];
            previousRect.GetWorldCorners(previousCorners);
            
            float previousTopY = previousCorners[2].y;
            Debug.Log($"TowerController: Base Y from previous block {previousBlock.GetColorName()}: {previousTopY}");
            return previousTopY;
        }

        private Vector3 CalculateNewPositionForBlock(BlockView blockView, float baseY)
        {
            RectTransform blockRect = blockView.GetRectTransform();
            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);
            
            float blockHeight = blockCorners[2].y - blockCorners[0].y;
            
            Vector3 currentPosition = blockView.transform.position;
            Vector3 newPosition = new Vector3(currentPosition.x, baseY + blockHeight / 2, currentPosition.z);
            
            return newPosition;
        }
    }
}