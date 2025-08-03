using System.Collections.Generic;
using System.Linq;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime;
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

        private TowerSaveData m_SaveData = new TowerSaveData();
        private TowerAreaDetector m_AreaDetector;
        private readonly List<BlockView> m_ActiveBlocks = new List<BlockView>();
        
        private int m_NextTowerBlockInstanceId = 1;
        
        private readonly Dictionary<BlockView, int> m_BlockToInstanceId = new Dictionary<BlockView, int>();

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
            Debug.Log(
                $"TowerController: Tower position: {towerParent.position}, Tower anchored pos: {((RectTransform)towerParent).anchoredPosition}");
            
            Vector3 originalPosition = blockView.transform.position;
            
            blockView.transform.position = position;
            
            bool isCompletelyInside = m_AreaDetector.IsBlockCompletelyInside(blockView.GetRectTransform());
            
            blockView.transform.position = originalPosition;

            Debug.Log($"TowerController: Block {blockView.GetColorName()} at {position} - " +
                      $"Completely inside: {isCompletelyInside}");

            return isCompletelyInside;
        }

        public bool TryPlaceBlockInTower(BlockView blockView, Vector3 position)
        {
            if (blockView == null)
            {
                Debug.LogError("TowerController: BlockView is null");
                return false;
            }

            if (!IsBlockInTowerArea(blockView, position))
            {
                Debug.Log($"TowerController: Block {blockView.GetColorName()} not completely in tower area");
                return false;
            }

            Debug.Log($"TowerController: Placing block at world position {position}");
    
            blockView.transform.position = position;
    
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
                
                int instanceId = m_NextTowerBlockInstanceId++;
                m_BlockToInstanceId[blockView] = instanceId;
        
                Debug.Log($"TowerController: Added block to active blocks with instance ID {instanceId}. Total count: {m_ActiveBlocks.Count}");
            }
            else
            {
                Debug.LogWarning($"TowerController: Block {blockView.GetColorName()} already in active blocks list");
            }
    
            SaveCurrentTowerState();
    
            Canvas.ForceUpdateCanvases();

            Debug.Log($"TowerController: Successfully placed block {blockView.GetColorName()} " +
                      $"at final position {blockView.transform.position}");

            return true;
        }

        private void SaveCurrentTowerState()
        {
            m_SaveData.TowerBlocks.Clear();
            
            var validBlocks = m_ActiveBlocks.Where(block => block != null).ToList();
            
            var nullBlocks = m_ActiveBlocks.Where(block => block == null).ToList();
            foreach (var nullBlock in nullBlocks)
            {
                m_ActiveBlocks.Remove(nullBlock);
            }
            
            var nullBlockKeys = m_BlockToInstanceId.Keys.Where(block => block == null).ToList();
            foreach (var nullKey in nullBlockKeys)
            {
                m_BlockToInstanceId.Remove(nullKey);
            }
            
            for (int i = 0; i < validBlocks.Count; i++)
            {
                var blockView = validBlocks[i];
                
                int instanceId = 0;
                if (m_BlockToInstanceId.ContainsKey(blockView))
                {
                    instanceId = m_BlockToInstanceId[blockView];
                }
                else
                {
                    instanceId = m_NextTowerBlockInstanceId++;
                    m_BlockToInstanceId[blockView] = instanceId;
                    Debug.LogWarning($"TowerController: Block {blockView.GetColorName()} was missing instance ID, assigned new: {instanceId}");
                }
                
                var blockData = new TowerBlockData(
                    blockView.GetId(),
                    blockView.transform.position,
                    i,
                    instanceId
                );
                
                m_SaveData.TowerBlocks.Add(blockData);
                Debug.Log($"TowerController: Saved block {blockView.GetColorName()} (instance {instanceId}) at position {blockView.transform.position}");
            }

            Debug.Log($"TowerController: Saved {m_SaveData.TowerBlocks.Count} blocks to save data (active blocks: {m_ActiveBlocks.Count})");
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
            
            if (m_SaveData.TowerBlocks.Count > 0)
            {
                m_NextTowerBlockInstanceId = m_SaveData.TowerBlocks.Max(b => b.TowerBlockInstanceId) + 1;
            }

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
                        Debug.Log($"TowerController: Restored block at world {savedWorldPosition}");
                    }
                    
                    m_ActiveBlocks.Add(blockView);
                    
                    m_BlockToInstanceId[blockView] = blockData.TowerBlockInstanceId;

                    Debug.Log(
                        $"TowerController: Restored block Config ID {blockData.BlockId} (instance {blockData.TowerBlockInstanceId}) at {savedWorldPosition} with DragType.Move");
                }
                else
                {
                    Debug.LogError($"TowerController: Failed to create block with Config ID {blockData.BlockId}");
                }
            }
            
            Canvas.ForceUpdateCanvases();

            Debug.Log($"TowerController: Successfully loaded {m_ActiveBlocks.Count} blocks. Next instance ID will be: {m_NextTowerBlockInstanceId}");
        }

        private void ClearCurrentBlocks()
        {
            foreach (var blockView in m_ActiveBlocks.Where(block => block != null))
            {
                Object.Destroy(blockView.gameObject);
            }

            m_ActiveBlocks.Clear();
            m_BlockToInstanceId.Clear();
            Debug.Log("TowerController: Cleared all current blocks and instance IDs");
        }

        public void RemoveBlockFromTower(BlockView blockView)
        {
            if (blockView == null)
            {
                Debug.LogWarning("TowerController: Attempted to remove null block from tower");
                return;
            }

            bool wasRemoved = m_ActiveBlocks.Remove(blockView);
            
            int instanceId = -1;
            if (m_BlockToInstanceId.ContainsKey(blockView))
            {
                instanceId = m_BlockToInstanceId[blockView];
                m_BlockToInstanceId.Remove(blockView);
            }
    
            if (wasRemoved)
            {
                Debug.Log($"TowerController: Removed block {blockView.GetColorName()} (instance {instanceId}) from tower. Remaining blocks: {m_ActiveBlocks.Count}");
                SaveCurrentTowerState();
            }
            else
            {
                Debug.LogWarning($"TowerController: Block {blockView.GetColorName()} was not found in active blocks list");
                
                int removedNulls = m_ActiveBlocks.RemoveAll(block => block == null);
                var nullKeys = m_BlockToInstanceId.Keys.Where(key => key == null).ToList();
                foreach (var nullKey in nullKeys)
                {
                    m_BlockToInstanceId.Remove(nullKey);
                }
        
                if (removedNulls > 0)
                {
                    Debug.Log($"TowerController: Cleaned up {removedNulls} null blocks from active list");
                    SaveCurrentTowerState();
                }
            }
        }
    }
}