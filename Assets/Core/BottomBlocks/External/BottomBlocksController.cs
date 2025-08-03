using System;
using Core.BottomBlocks.Runtime;
using Core.UI.Runtime;
using UnityEngine;
using Zenject;

namespace Core.BottomBlocks.External
{
    public class BottomBlocksController : IInitializable, IDisposable
    {
        [Inject] private ICoreUIController m_CoreUIController;
        [Inject] private IBlockFactoryController m_BlockFactoryController;

        private BottomBlocksView m_View;
        private ScrollBlocker m_ScrollBlocker = new ScrollBlocker();

        public void Initialize()
        {
            if (!TryGetBottomBlocksView())
            {
                Debug.LogError("BottomBlocksController: Failed to get BottomBlocksView");
                return;
            }

            m_ScrollBlocker.Init(m_View.GetScrollRect());

            ScrollEvents.OnBlockScrollRequested += BlockScroll;
            ScrollEvents.OnUnblockScrollRequested += UnblockScroll;
            
            CreateAllBlocks();
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

        private void CreateAllBlocks()
        {
            try
            {
                var createdBlocks = m_BlockFactoryController.CreateAllBlocksFromConfig(m_View.GetScrollContent());
                
                Debug.Log($"BottomBlocksController: Successfully created {createdBlocks.Count} blocks for bottom panel");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BottomBlocksController: Error creating blocks - {ex.Message}");
            }
        }

        private void BlockScroll()
        {
            m_ScrollBlocker?.BlockScroll();
        }

        private void UnblockScroll()
        {
            m_ScrollBlocker?.UnblockScroll();
        }

        public void Dispose()
        {
            ScrollEvents.OnBlockScrollRequested -= BlockScroll;
            ScrollEvents.OnUnblockScrollRequested -= UnblockScroll;
        }
    }
}