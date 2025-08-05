using System.Linq;
using Core.BottomBlocks.Runtime;
using Core.DragAndDrop.Runtime;
using Core.SaveSystem.Runtime;
using Core.Tower.Runtime;
using Core.Tower.Runtime.Data;
using Core.UI.Runtime;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerSaveManager : ITowerSaveManager
    {
        private readonly TowerSaveData m_SaveData;
        private readonly IAutoSaveController m_SaveController;
        private readonly IBlockFactoryController m_BlockFactoryController;
        private readonly ICoreUIController m_CoreUIController;

        public TowerSaveManager(
            TowerSaveData saveData,
            IAutoSaveController saveController,
            IBlockFactoryController blockFactoryController,
            ICoreUIController coreUIController)
        {
            m_SaveData = saveData;
            m_SaveController = saveController;
            m_BlockFactoryController = blockFactoryController;
            m_CoreUIController = coreUIController;
        }

        public void SaveTowerState(TowerState towerState)
        {
            m_SaveData.TowerBlocks.Clear();

            var validBlocks = towerState.ActiveBlocks.Where(block => block != null).ToList();

            for (int i = 0; i < validBlocks.Count; i++)
            {
                var blockView = validBlocks[i];
                var blockData = new TowerBlockData(
                    blockView.GetId(),
                    blockView.transform.position,
                    i
                );

                m_SaveData.TowerBlocks.Add(blockData);
                Debug.Log($"TowerSaveManager: Saved block {blockView.GetColorName()} at layer {i}");
            }

            m_SaveController.SaveData(m_SaveData);
            Debug.Log($"TowerSaveManager: Saved {m_SaveData.TowerBlocks.Count} blocks");
        }

        public void LoadTowerState(TowerState towerState)
        {
            Debug.Log($"TowerSaveManager: Loading {m_SaveData.TowerBlocks.Count} blocks");

            ClearCurrentBlocks(towerState);

            var coreView = m_CoreUIController.GetCoreView();
            var towerParent = coreView.DraggingBlockView;

            foreach (var blockData in m_SaveData.TowerBlocks.OrderBy(b => b.LayerIndex))
            {
                var blockView = m_BlockFactoryController.CreateBlockById(
                    blockData.BlockId,
                    towerParent,
                    DragType.Move
                );

                if (blockView != null)
                {
                    blockView.transform.position = blockData.GetPosition();

                    var blockRect = blockView.GetRectTransform();
                    blockRect?.ForceUpdateRectTransforms();

                    towerState.AddBlock(blockView);

                    Debug.Log($"TowerSaveManager: Restored block {blockData.BlockId} at layer {blockData.LayerIndex}");
                }
                else
                {
                    Debug.LogError($"TowerSaveManager: Failed to create block {blockData.BlockId}");
                }
            }

            Canvas.ForceUpdateCanvases();
            Debug.Log($"TowerSaveManager: Loaded {towerState.ActiveBlocks.Count} blocks");
        }

        private void ClearCurrentBlocks(TowerState towerState)
        {
            foreach (var blockView in towerState.ActiveBlocks.Where(block => block != null))
            {
                Object.Destroy(blockView.gameObject);
            }

            towerState.ClearBlocks();
            Debug.Log("TowerSaveManager: Cleared all blocks");
        }
    }
}