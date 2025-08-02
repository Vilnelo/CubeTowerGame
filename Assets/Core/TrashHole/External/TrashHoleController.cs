using Core.Animations.External;
using Core.Canvases.Runtime;
using Core.TrashHole.Runtime;
using Core.UI.External;
using Core.UI.Runtime;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Core.TrashHole.External
{
    public class TrashHoleController : ITrashHoleController, IInitializable
    {
        [Inject] private ICoreUIController m_CoreUIController;
        [Inject] private IMainCanvas m_MainCanvas;
        
        private TrashHoleView m_TrashHoleView;
        private TrashHoleDetector m_TrashHoleDetector;
        
        public void Initialize()
        {
            InitializeTrashHole();
            
            Debug.Log("TrashHoleController: Initialized");
        }
        
        private void InitializeTrashHole()
        {
            var coreView = m_CoreUIController.GetCoreView();
            m_TrashHoleView = coreView.TrashHoleView;
            
            if (m_TrashHoleView == null)
            {
                Debug.LogError("TrashHoleController: TrashHoleView not found in CoreView!");
                return;
            }
            
            m_TrashHoleDetector = new TrashHoleDetector(
                m_TrashHoleView.GetTrashHoleRect(),
                m_TrashHoleView.GetOvalSize()
            );
        }
        
        public bool IsBlockTouchingHole(RectTransform blockRect)
        {
            if (m_TrashHoleDetector == null)
            {
                Debug.LogWarning("TrashHoleController: Detector not initialized!");
                return false;
            }
            
            return m_TrashHoleDetector.IsBlockTouchingHole(blockRect);
        }
        
        public Vector3 GetHoleBottomWorldPosition()
        {
            if (m_TrashHoleDetector == null)
            {
                Debug.LogWarning("TrashHoleController: Detector not initialized!");
                return Vector3.zero;
            }
            
            return m_TrashHoleDetector.GetHoleBottomWorldPosition();
        }
        
        public void DestroyBlockInHole(GameObject block)
        {
            if (block == null)
                return;
                
            Debug.Log($"TrashHoleController: Starting fall animation for block {block.name}");
            
            // Создаем анимацию падения в яму
            var fallAnimation = new TrashHoleFallAnimation();
            Vector3 holeBottomWorldPosition = GetHoleBottomWorldPosition();
            
            // Запускаем анимацию с callback на уничтожение
            fallAnimation.StartFallAnimation(block, holeBottomWorldPosition, m_MainCanvas.GetCanvas(), () =>
            {
               
            }, () =>
            {
                // Этот callback вызовется когда анимация закончится
                Debug.Log($"TrashHoleController: Animation completed, destroying block {block.name}");
                
                if (block != null)
                {
                    Object.Destroy(block);
                }
                
                // Освобождаем ресурсы анимации
                fallAnimation.Dispose();
            });
        }
    }
}