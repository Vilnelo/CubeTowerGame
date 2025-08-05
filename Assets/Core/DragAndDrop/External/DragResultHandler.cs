using Core.Animations.External.PopupText.External;
using Core.BottomBlocks.Runtime;
using Core.DragAndDrop.Runtime;
using Core.Localization.Runtime;
using Core.Tower.Runtime;
using Core.TrashHole.Runtime;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class DragResultHandler : IDragResultHandler
    {
        private readonly ITrashHoleController m_TrashHoleController;
        private readonly ITowerController m_TowerController;
        private readonly IAnimationManager m_AnimationManager;

        public DragResultHandler(
            ITrashHoleController trashHoleController,
            ITowerController towerController,
            IAnimationManager animationManager)
        {
            m_TrashHoleController = trashHoleController;
            m_TowerController = towerController;
            m_AnimationManager = animationManager;
        }

        public void HandleDragResult(DragResult result)
        {
            var animation = m_AnimationManager.GetOrCreatePickupAnimation(result.BlockView);

            switch (result.ResultType)
            {
                case DragResultType.TrashDestruction:
                    animation.ScaleDown(() => HandleTrashDestruction(result));
                    break;
                case DragResultType.TowerPlacement:
                    animation.ScaleDown(() => HandleTowerPlacement(result));
                    break;
                case DragResultType.RegularDestruction:
                    animation.ScaleDown(() => HandleRegularDestruction(result));
                    break;
            }
        }

        private void HandleTrashDestruction(DragResult result)
        {
            m_TrashHoleController.DestroyBlockInHole(result.DraggedObject);
            m_AnimationManager.CleanupPickupAnimation(result.BlockView);
            PopupTextController.ShowPopupText(LocalizationKeys.BlockTrashedTextKey.Localize());

            ScrollEvents.RequestUnblockScroll();
            result.OnComplete?.Invoke();
            Debug.Log("DragResultHandler: Block destroyed in trash hole");
        }

        private void HandleTowerPlacement(DragResult result)
        {
            if (m_TowerController.TryPlaceBlockInTower(result.BlockView, result.Position))
            {
                m_AnimationManager.CleanupPickupAnimation(result.BlockView);
                PopupTextController.ShowPopupText(LocalizationKeys.BlockPlacedTextKey.Localize());

                result.BlockView.gameObject.SetActive(true);
                var canvasRenderer = result.BlockView.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetAlpha(1.0f);
                }

                Canvas.ForceUpdateCanvases();
                m_TowerController.SaveTowerState();
                Debug.Log($"DragResultHandler: Block {result.BlockView.GetColorName()} placed in tower");
            }
            else
            {
                Debug.LogWarning("DragResultHandler: Failed to place block in tower - destroying");
                HandleRegularDestruction(result);
                return;
            }

            ScrollEvents.RequestUnblockScroll();
            result.OnComplete?.Invoke();
        }

        private void HandleRegularDestruction(DragResult result)
        {
            var draggable = result.BlockView.GetDraggableBlockController();
            draggable.SetDragType(DragType.Destroying);

            PopupTextController.ShowPopupText(LocalizationKeys.BlockDestroyedTextKey.Localize());

            var destructionAnimation = m_AnimationManager.GetOrCreateDestructionAnimation(result.BlockView);
            destructionAnimation.StartDestruction(() =>
            {
                if (result.DraggedObject != null)
                {
                    Object.Destroy(result.DraggedObject);
                }

                m_AnimationManager.CleanupPickupAnimation(result.BlockView);
                m_AnimationManager.CleanupDestructionAnimation(result.BlockView);

                ScrollEvents.RequestUnblockScroll();
                result.OnComplete?.Invoke();
                Debug.Log("DragResultHandler: Block destroyed with animation");
            });

            Debug.Log("DragResultHandler: Started block destruction animation");
        }
    }
}