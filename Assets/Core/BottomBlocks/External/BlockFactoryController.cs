using System.Collections.Generic;
using System.Linq;
using Core.AssetManagement.Runtime;
using Core.BottomBlocks.Runtime;
using Core.BottomBlocks.Runtime.Dto;
using Core.ConfigSystem.Runtime;
using Core.DragAndDrop.Runtime;
using Core.UI.Runtime;
using UnityEngine;
using Zenject;

namespace Core.BottomBlocks.External
{
    public class BlockFactoryController : IBlockFactoryController, IInitializable
    {
        [Inject] private IConfigLoader m_ConfigLoader;
        [Inject] private IAssetLoader m_AssetLoader;
        [Inject] private ILayoutUIController m_LayoutUIController;

        private const string m_BlocksConfigKey = "BlocksConfig";
        private const string m_BottomBlockViewKey = "BottomBlockView";

        private CubesDto m_CubesConfig;

        public void Initialize()
        {
            m_CubesConfig = m_ConfigLoader.GetConfig<CubesDto>(m_BlocksConfigKey);

            if (m_CubesConfig == null || m_CubesConfig.Cubes == null)
            {
                Debug.LogError("BlockFactoryController: Failed to load cubes config");
                return;
            }

            Debug.Log($"BlockFactoryController: Initialized with {m_CubesConfig.Cubes.Count} cube configurations");
        }

        public BlockView CreateBlock(CubeDto cubeConfig, Transform parent, DragType dragType)
        {
            if (cubeConfig == null)
            {
                Debug.LogError("BlockFactoryController: Cannot create block - cubeConfig is null");
                return null;
            }

            try
            {
                var blockView = m_AssetLoader.InstantiateSync<BlockView>(
                    m_BottomBlockViewKey,
                    parent
                );

                if (blockView != null)
                {
                    blockView.Init(cubeConfig);
                    LoadAndSetSprite(blockView, cubeConfig.SpriteName);
                    SetupBlockWithLayout(blockView);
                    
                    var draggableController = blockView.GetDraggableBlockController();
                    if (draggableController != null)
                    {
                        draggableController.SetDragType(dragType);
                        Debug.Log($"BlockFactoryController: Set DragType.{dragType} for block {cubeConfig.ColorName}");
                    }

                    Debug.Log($"BlockFactoryController: Created block {cubeConfig.ColorName} (ID: {cubeConfig.Id}) with DragType.{dragType}");
                }

                return blockView;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BlockFactoryController: Failed to create block {cubeConfig.ColorName} - {ex.Message}");
                return null;
            }
        }

        public BlockView CreateBlockById(int blockId, Transform parent, DragType dragType)
        {
            var cubeConfig = GetCubeConfigById(blockId);
            if (cubeConfig == null)
            {
                Debug.LogError($"BlockFactoryController: No cube config found for ID {blockId}");
                return null;
            }

            return CreateBlock(cubeConfig, parent, dragType);
        }

        public List<BlockView> CreateAllBlocksFromConfig(Transform parent)
        {
            var createdBlocks = new List<BlockView>();

            if (m_CubesConfig?.Cubes == null)
            {
                Debug.LogError("BlockFactoryController: No cubes config available");
                return createdBlocks;
            }
            
            for (int i = m_CubesConfig.Cubes.Count - 1; i >= 0; i--)
            {
                var blockView = CreateBlock(m_CubesConfig.Cubes[i], parent, DragType.Clone);
                if (blockView != null)
                {
                    createdBlocks.Add(blockView);
                }
            }

            Debug.Log($"BlockFactoryController: Created {createdBlocks.Count} blocks from config for bottom panel");
            return createdBlocks;
        }

        public CubeDto GetCubeConfigById(int blockId)
        {
            if (m_CubesConfig?.Cubes == null)
            {
                Debug.LogError("BlockFactoryController: Cubes config not loaded");
                return null;
            }

            return m_CubesConfig.Cubes.FirstOrDefault(cube => cube.Id == blockId);
        }

        public bool HasCubeConfig(int blockId)
        {
            return GetCubeConfigById(blockId) != null;
        }

        public int GetTotalBlocksCount()
        {
            return m_CubesConfig?.Cubes?.Count ?? 0;
        }

        private void LoadAndSetSprite(BlockView blockView, string spriteName)
        {
            try
            {
                var sprite = m_AssetLoader.LoadSync<Sprite>(spriteName);

                if (sprite != null)
                {
                    blockView.SetImage(sprite);
                }
                else
                {
                    Debug.LogError($"BlockFactoryController: Failed to load sprite {spriteName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BlockFactoryController: Error loading sprite {spriteName} - {ex.Message}");
            }
        }

        private void SetupBlockWithLayout(BlockView blockView)
        {
            float blockSize = m_LayoutUIController.GetCubeSize();
            blockView.SetSize(blockSize);
        }
    }
}