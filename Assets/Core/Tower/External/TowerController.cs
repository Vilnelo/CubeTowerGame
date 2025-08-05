using System.Collections.Generic;
using System.Linq;
using Core.Animations.External;
using Core.Animations.External.PopupText.External;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime;
using Core.Canvases.Runtime;
using Core.DragAndDrop.Runtime;
using Core.Localization.Runtime;
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

        private TowerState m_TowerState;
        private TowerAreaDetector m_AreaDetector;
        private ITowerPlacementValidator m_PlacementValidator;
        private ITowerPositionCalculator m_PositionCalculator;
        private ITowerAnimationManager m_AnimationManager;
        private ITowerSaveManager m_SaveManager;
        private ITowerBlockOperations m_BlockOperations;
        private TowerSaveData m_SaveData;

        public void Initialize()
        {
            Debug.Log("TowerController: Initializing...");

            InitializeDependencies();
            SetupAreaDetector();
            RegisterSaveData();
            LoadTowerState();

            Debug.Log("TowerController: Initialized successfully");
        }

        private void InitializeDependencies()
        {
            m_TowerState = new TowerState();
            m_SaveData = new TowerSaveData();
        }

        private void SetupAreaDetector()
        {
            var coreView = m_CoreUIController.GetCoreView();
            if (coreView?.TowerView?.Tower != null)
            {
                m_AreaDetector = new TowerAreaDetector(coreView.TowerView.Tower);

                m_PlacementValidator = new TowerPlacementValidator(m_AreaDetector);
                m_PositionCalculator = new TowerPositionCalculator(m_AreaDetector, coreView.TowerView.Tower);
                m_AnimationManager = new TowerAnimationManager(m_TowerState, m_MainCanvas);
                m_SaveManager = new TowerSaveManager(m_SaveData, m_SaveController, m_BlockFactoryController,
                    m_CoreUIController);
                m_BlockOperations = new TowerBlockOperations(m_PlacementValidator, m_PositionCalculator,
                    m_AnimationManager, m_SaveManager, coreView.TowerView.Tower);

                Debug.Log("TowerController: All components initialized");
            }
            else
            {
                Debug.LogError("TowerController: Tower area not found!");
            }
        }

        private void RegisterSaveData()
        {
            m_SaveController.RegisterAutoSave(m_SaveData);
        }

        public bool IsBlockInTowerArea(BlockView blockView, Vector3 position)
        {
            if (m_AreaDetector == null || blockView == null)
            {
                Debug.LogWarning("TowerController: AreaDetector or BlockView is null");
                return false;
            }

            Debug.Log($"TowerController: Checking position {position} for block {blockView.GetColorName()}");

            Vector3 originalPosition = blockView.transform.position;
            blockView.transform.position = position;

            bool isCompletelyInside = m_AreaDetector.IsBlockCompletelyInside(blockView.GetRectTransform());

            blockView.transform.position = originalPosition;

            Debug.Log($"TowerController: Block at {position} - Inside area: {isCompletelyInside}");
            return isCompletelyInside;
        }

        public bool IsAnyAnimationPlaying()
        {
            return m_AnimationManager?.IsAnyAnimationPlaying() ?? false;
        }

        public bool CanPlaceBlockInTower(BlockView blockView, Vector3 position)
        {
            if (m_PlacementValidator == null)
            {
                Debug.LogError("TowerController: PlacementValidator not initialized");
                return false;
            }

            bool canPlace = m_PlacementValidator.CanPlaceBlock(blockView, position, m_TowerState);
            Debug.Log($"TowerController: Can place block {blockView?.GetColorName()}: {canPlace}");
            return canPlace;
        }

        public bool TryPlaceBlockInTower(BlockView blockView, Vector3 position)
        {
            if (m_BlockOperations == null)
            {
                Debug.LogError("TowerController: BlockOperations not initialized");
                return false;
            }

            if (!CanPlaceBlockInTower(blockView, position))
            {
                Debug.Log($"TowerController: Cannot place block {blockView?.GetColorName()} - validation failed");
                return false;
            }

            try
            {
                bool placed = m_BlockOperations.PlaceBlock(blockView, position, m_TowerState);
                if (placed)
                {
                    Debug.Log($"TowerController: Successfully placed block {blockView.GetColorName()}");
                }
                else
                {
                    Debug.Log($"TowerController: Failed to place block {blockView.GetColorName()}");
                }

                return placed;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"TowerController: Error placing block: {ex.Message}");
                return false;
            }
        }

        public void RemoveBlockFromTower(BlockView blockView)
        {
            if (m_BlockOperations == null)
            {
                Debug.LogError("TowerController: BlockOperations not initialized");
                return;
            }

            Debug.Log($"TowerController: Removing block {blockView?.GetColorName()} from tower");
            m_BlockOperations.RemoveBlock(blockView, m_TowerState);
        }

        public void SaveTowerState()
        {
            if (m_SaveManager == null)
            {
                Debug.LogError("TowerController: SaveManager not initialized");
                return;
            }

            m_SaveManager.SaveTowerState(m_TowerState);
            Debug.Log($"TowerController: Manually saved tower state with {m_TowerState.ActiveBlocks.Count} blocks");
        }

        public void LoadTowerState()
        {
            if (m_SaveManager == null)
            {
                Debug.LogError("TowerController: SaveManager not initialized");
                return;
            }

            m_SaveManager.LoadTowerState(m_TowerState);
            Debug.Log($"TowerController: Loaded tower state with {m_TowerState.ActiveBlocks.Count} blocks");
        }
    }
}