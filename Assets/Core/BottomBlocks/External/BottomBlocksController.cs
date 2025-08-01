using Core.AssetManagement.Runtime;
using Core.BottomBlocks.Runtime.Dto;
using Core.ConfigSystem.Runtime;
using Core.DragAndDrop.Runtime;
using Core.UI.Runtime;
using UnityEngine;
using Zenject;

namespace Core.BottomBlocks.External
{
    public class BottomBlocksController : IInitializable
    {
        [Inject] private IConfigLoader m_ConfigLoader;
        [Inject] private ILayoutUIController m_LayoutUIController;
        [Inject] private IAssetLoader m_AssetLoader;
        [Inject] private ICoreUIController m_CoreUIController;
        
        private const string m_BlocksConfigKey = "BlocksConfig";
        private const string m_BottomBlockViewKey = "BottomBlockView";
        
        private BottomBlocksView m_View;
        private ScrollBlocker m_ScrollBlocker = new ScrollBlocker();
        private CubesDto m_CubesDto;
        
        public void Initialize()
        {
            var cubesConfig = m_ConfigLoader.GetConfig<CubesDto>(m_BlocksConfigKey);
            
            if (cubesConfig == null)
            {
                Debug.LogError("BottomBlocksController: Failed to load cubes config");
                return;
            }
            
            if (!TryGetBottomBlocksView())
            {
                Debug.LogError("BottomBlocksController: Failed to get BottomBlocksView");
                return;
            }
            
            m_ScrollBlocker.Init(m_View.GetScrollRect());
            
            Debug.Log($"BottomBlocksController: Loaded {cubesConfig.Cubes.Count} cubes configuration");
            
            CreateBlocksFromConfig(cubesConfig);
        }
        
        private bool TryGetBottomBlocksView()
        {
            var coreView = m_CoreUIController.GetCoreView();
            if (coreView == null)
            {
                Debug.LogError("BottomBlocksController: CoreUIView is null");
                return false;
            }

            m_View = coreView.BottomBlocksView;
            if (m_View == null)
            {
                Debug.LogError("BottomBlocksController: BottomBlocksView is null in CoreUIView");
                return false;
            }

            if (m_View.GetScrollContent() == null)
            {
                Debug.LogError("BottomBlocksController: ScrollContent is null in BottomBlocksView");
                return false;
            }

            Debug.Log("BottomBlocksController: Successfully got BottomBlocksView");
            return true;
        }
        
        private void CreateBlocksFromConfig(CubesDto config)
        {
            for (int i = config.Cubes.Count - 1; i >= 0; i--)
            {
                CreateBlock(config.Cubes[i]);
            }
        }
        
        private void CreateBlock(CubeDto cubeConfig)
        {
            try
            {
                var blockView = m_AssetLoader.InstantiateSync<BlockView>(
                    m_BottomBlockViewKey, 
                    m_View.GetScrollContent()
                );
                
                if (blockView != null)
                {
                    blockView.Init(cubeConfig);
                    
                    LoadAndSetSprite(blockView, cubeConfig.SpriteName);
                    
                    blockView.GetDraggableBlockController().SetDragType(DragType.Clone);
                    
                    SetupBlockWithLayout(blockView, cubeConfig);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BottomBlocksController: Failed to create block {cubeConfig.ColorName} - {ex.Message}");
            }
        }
        
        private void LoadAndSetSprite(BlockView blockView, string spriteName)
        {
            try
            {
                var sprite = m_AssetLoader.LoadSync<Sprite>(spriteName);
                
                if (sprite != null)
                {
                    blockView.SetImage(sprite);
                    Debug.Log($"BottomBlocksController: Loaded sprite {spriteName}");
                }
                else
                {
                    Debug.LogError($"BottomBlocksController: Failed to load sprite {spriteName}, using fallback color");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BottomBlocksController: Error loading sprite {spriteName} - {ex.Message}");
            }
        }
        
        private void SetupBlockWithLayout(BlockView blockView, CubeDto config)
        {
            float blockSize = m_LayoutUIController.GetCubeSize();
            blockView.SetSize(blockSize);
            
            Debug.Log($"BottomBlocksController: Created block {config.ColorName} with size {blockSize:F2}");
        }
    }
}