using System;
using System.Collections.Generic;
using Core.Animations.External;
using Core.Animations.External.PopupText.External;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime;
using Core.DragAndDrop.Runtime;
using Core.InputSystem.External;
using Core.Tower.Runtime;
using Core.TrashHole.Runtime;
using Core.UI.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.SimpleTimerSystem.External;
using Utils.SimpleTimerSystem.Runtime;
using Zenject;
using Object = UnityEngine.Object;

namespace Core.DragAndDrop.External
{
    public class DragAndDropController : IInitializable, IDisposable
    {
        [Inject] private ICoreUIController m_CoreUIController;
        [Inject] private ITimerController m_TimerController;
        [Inject] private ITrashHoleController m_TrashHoleController;
        [Inject] private ITowerController m_TowerController;
        [Inject] private IBlockFactoryController m_BlockFactoryController;

        private GameObject m_CurrentDraggedObject;
        private Camera m_MainCamera;
        private SimpleTimerWrapper m_CountdownTimer;
        private BlockView m_CurrentBlockView;
        private bool m_IsWaitingForPickup = false;
        private bool m_IsDragging = false;
        private bool m_HasPickedUpBlock = false;

        private Dictionary<RectTransform, PickUpAnimation> m_BlockAnimations =
            new Dictionary<RectTransform, PickUpAnimation>();

        private Dictionary<RectTransform, DestructionAnimation> m_DestructionAnimations =
            new Dictionary<RectTransform, DestructionAnimation>();

        private const float m_LerpForce = 1000f;
        private const float m_DelayTime = 0.3f;

        public void Initialize()
        {
            Debug.Log("DragAndDropSystem: Initialized");

            SubscribeToInputEvents();
            CreateTimer();
        }

        private void SubscribeToInputEvents()
        {
            InputManager.OnMouseDown += OnMouseDown;
            InputManager.OnMouseUp += OnMouseUp;
            InputManager.OnStartDrag += OnStartDrag;
            InputManager.OnDragging += OnDragging;
            InputManager.OnEndDrag += OnEndDrag;
        }

        private void CreateTimer()
        {
            m_CountdownTimer = new SimpleTimerWrapper(
                m_TimerController,
                m_DelayTime,
                OnTimerTick,
                OnTimerComplete
            );
        }

        private PickUpAnimation GetOrCreateAnimation(BlockView blockView)
        {
            var rectTransform = blockView.GetRectTransform();

            if (!m_BlockAnimations.ContainsKey(rectTransform))
            {
                var animation = new PickUpAnimation();
                animation.Initialize(rectTransform);
                m_BlockAnimations[rectTransform] = animation;
                Debug.Log("DragAndDropSystem: Created new animation for block");
            }

            return m_BlockAnimations[rectTransform];
        }

        private DestructionAnimation GetOrCreateDestructionAnimation(BlockView blockView)
        {
            var rectTransform = blockView.GetRectTransform();

            if (!m_DestructionAnimations.ContainsKey(rectTransform))
            {
                var animation = new DestructionAnimation();
                animation.Initialize(rectTransform);
                m_DestructionAnimations[rectTransform] = animation;
                Debug.Log("DragAndDropSystem: Created new destruction animation for block");
            }

            return m_DestructionAnimations[rectTransform];
        }

        private void CleanupAnimation(BlockView blockView)
        {
            if (blockView == null) return;

            var rectTransform = blockView.GetRectTransform();
            if (m_BlockAnimations.ContainsKey(rectTransform))
            {
                m_BlockAnimations[rectTransform].Dispose();
                m_BlockAnimations.Remove(rectTransform);
                Debug.Log("DragAndDropSystem: Cleaned up animation for block");
            }
        }

        private void CleanupDestructionAnimation(BlockView blockView)
        {
            if (blockView == null) return;

            var rectTransform = blockView.GetRectTransform();
            if (m_DestructionAnimations.ContainsKey(rectTransform))
            {
                m_DestructionAnimations[rectTransform].Dispose();
                m_DestructionAnimations.Remove(rectTransform);
                Debug.Log("DragAndDropSystem: Cleaned up destruction animation for block");
            }
        }

        private void StartPickUpTimer()
        {
            m_CountdownTimer.StopTimer();
            m_CountdownTimer.ResetTimer();
            m_CountdownTimer.StartTimer();
            m_IsWaitingForPickup = true;
        }

        private void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            // Do nothing
        }

        private void OnTimerComplete(SimpleTimerInfo timerInfo)
        {
            if (m_IsWaitingForPickup && m_CurrentBlockView != null)
            {
                ScrollEvents.RequestBlockScroll();
                PickUpBlock(m_CurrentBlockView);
                m_IsWaitingForPickup = false;
                Debug.Log("DragAndDropSystem: Timer completed - block picked up and scroll blocked");
            }

            m_CountdownTimer.StopTimer();
        }

        private void OnMouseDown(Vector3 worldPosition)
        {
            var blockView = FindDraggableAtPosition(worldPosition);

            if (blockView == null)
            {
                return;
            }

            if (m_CurrentBlockView != null && m_CurrentBlockView != blockView)
            {
                var previousAnimation = GetOrCreateAnimation(m_CurrentBlockView);
                previousAnimation.ForceResetToReferenceScale();
            }

            var animation = GetOrCreateAnimation(blockView);
            
            if (animation.IsAnimating())
            {
                Debug.Log("DragAndDropSystem: Animation in progress - ignoring click");
                return;
            }
            
            if (!animation.IsAtReferenceScale())
            {
                Debug.Log("DragAndDropSystem: Block not at reference scale - ignoring click");
                return;
            }

            var draggable = blockView.GetDraggableBlockController();
            var dragBehavior = draggable.GetDragBehavior();

            if (dragBehavior == DragType.Destroying)
            {
                Debug.Log("DragAndDropSystem: Block is being destroyed - ignoring click");
                return;
            }

            m_CurrentBlockView = blockView;

            if (dragBehavior == DragType.Move)
            {
                ScrollEvents.RequestBlockScroll();
                PickUpBlock(blockView);
                Debug.Log("DragAndDropSystem: Move block picked up immediately");
            }
            else if (dragBehavior == DragType.Clone)
            {
                StartPickUpTimer();
                Debug.Log("DragAndDropSystem: Clone block - timer started for pickup");
            }
        }

        private void OnMouseUp(Vector3 worldPosition)
        {
            if (m_IsWaitingForPickup)
            {
                m_CountdownTimer.StopTimer();
                m_IsWaitingForPickup = false;
                ScrollEvents.RequestUnblockScroll();
                ResetDragState();
                Debug.Log("DragAndDropSystem: Mouse up - timer stopped");
                return;
            }
    
            if (m_HasPickedUpBlock && !m_IsDragging && m_CurrentBlockView != null)
            {
                var dragBehavior = m_CurrentBlockView.GetDraggableBlockController().GetDragBehavior();
                if (dragBehavior == DragType.Move)
                {
                    Vector3 currentPosition = m_CurrentBlockView.transform.position;
                    if (m_TowerController.IsBlockInTowerArea(m_CurrentBlockView, currentPosition))
                    {
                        m_TowerController.TryPlaceBlockInTower(m_CurrentBlockView, currentPosition);
                        m_TowerController.SaveTowerState();
                        Debug.Log("DragAndDropSystem: Block returned to tower at original position");
                    }
                }
        
                var animation = GetOrCreateAnimation(m_CurrentBlockView);
                animation.ScaleDown(() => {
                    ScrollEvents.RequestUnblockScroll();
                    ResetDragState();
                });

                Debug.Log("DragAndDropSystem: Mouse up without drag - scaling down with callback");
            }
        }

        private void PickUpBlock(BlockView blockView)
        {
            var dragBehavior = m_CurrentBlockView.GetDraggableBlockController().GetDragBehavior();

            if (dragBehavior == DragType.Clone)
            {
                CreateDraggedObject(blockView);
            }
            else if (dragBehavior == DragType.Move)
            {
                m_TowerController.RemoveBlockFromTower(m_CurrentBlockView);
                m_TowerController.SaveTowerState();
        
                m_CurrentDraggedObject = m_CurrentBlockView.GetDraggableBlockController().GetGameObject();
            }

            if (m_CurrentDraggedObject != null)
            {
                m_CurrentDraggedObject.transform.SetAsLastSibling();
            }

            var animation = GetOrCreateAnimation(m_CurrentBlockView);
            animation.ScaleUp();

            m_HasPickedUpBlock = true;

            m_CurrentBlockView.GetDraggableBlockController().OnDragStart();

            //TODO: .Localize()
            PopupTextController.ShowPopupText(TextBlockConstants.BlockPickedUpText);
        }

        private void OnStartDrag(Vector3 worldPosition)
        {
            if (m_IsWaitingForPickup)
            {
                m_CountdownTimer.StopTimer();
                m_IsWaitingForPickup = false;
                Debug.Log("DragAndDropSystem: Drag started before timer - timer stopped");
                return;
            }

            if (m_CurrentDraggedObject != null)
            {
                m_IsDragging = true;
                Debug.Log("DragAndDropSystem: Drag actually started");
            }
        }

        private void OnDragging(Vector3 worldPosition)
        {
            if (m_CurrentDraggedObject != null && m_IsDragging)
            {
                UpdateDraggedObjectPosition(worldPosition);
            }
        }

        private void OnEndDrag(Vector3 worldPosition)
        {
            if (m_IsWaitingForPickup)
            {
                m_CountdownTimer.StopTimer();
                m_IsWaitingForPickup = false;
                ScrollEvents.RequestUnblockScroll();
                ResetDragState();
                Debug.Log("DragAndDropSystem: Mouse released - timer stopped");
                return;
            }

            if (m_CurrentDraggedObject != null && m_IsDragging && m_CurrentBlockView != null)
            {
                FinishDragging(worldPosition);
            }
            else
            {
                ScrollEvents.RequestUnblockScroll();
                ResetDragState();
            }
        }

        private void ResetDragState()
        {
            if (m_CurrentBlockView != null)
            {
                var animation = GetOrCreateAnimation(m_CurrentBlockView);
                if (!animation.IsAtReferenceScale())
                {
                    animation.ForceResetToReferenceScale();
                    Debug.Log("DragAndDropSystem: Force reset scale in ResetDragState");
                }
            }

            m_CurrentBlockView = null;
            m_CurrentDraggedObject = null;
            m_IsDragging = false;
            m_IsWaitingForPickup = false;
            m_HasPickedUpBlock = false;
        }

        private BlockView FindDraggableAtPosition(Vector3 worldPosition)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> resultsData = new List<RaycastResult>();
            pointerData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointerData, resultsData);

            foreach (var result in resultsData)
            {
                if (result.gameObject.transform.TryGetComponent<BlockView>(out var blockView))
                {
                    return blockView;
                }
            }

            return null;
        }

        private void CreateDraggedObject(BlockView blockView)
        {
            var originalBlock = blockView;
            if (originalBlock == null)
            {
                Debug.LogError("DragAndDropSystem: Can only drag BlockView objects for now");
                return;
            }
            
            var originalId = originalBlock.GetId();
            
            var draggedBlockView = m_BlockFactoryController.CreateBlockById(
                originalId,
                m_CoreUIController.GetCoreView().DraggingBlockView,
                DragType.Move
            );

            if (draggedBlockView != null)
            {
                draggedBlockView.transform.position = originalBlock.GetRectTransform().position;
                draggedBlockView.transform.rotation = originalBlock.GetRectTransform().rotation;

                m_CurrentDraggedObject = draggedBlockView.gameObject;
                m_CurrentBlockView = draggedBlockView;

                Debug.Log($"DragAndDropSystem: Created cloned object with ID {originalId} and DragType.Move");
            }
            else
            {
                Debug.LogError($"DragAndDropSystem: Failed to create cloned block with ID {originalId}");
            }
        }

        private void UpdateDraggedObjectPosition(Vector3 worldPosition)
        {
            if (m_CurrentDraggedObject != null)
            {
                var followedPosition = m_CurrentDraggedObject.transform.position;
                var dragPosition = new Vector3(worldPosition.x, worldPosition.y, followedPosition.z);
                m_CurrentDraggedObject.transform.position = Vector3.Lerp(followedPosition, dragPosition,
                    Time.deltaTime * m_LerpForce);
            }
        }

        private void FinishDragging(Vector3 endPosition)
        {
            if (m_CurrentBlockView == null || m_CurrentBlockView.GetDraggableBlockController() == null)
            {
                Debug.LogWarning("DragAndDropSystem: CurrentBlockView or DraggableBlockController is null");
                ScrollEvents.RequestUnblockScroll();
                ResetDragState();
                return;
            }

            bool shouldDestroy = false;
            bool isInTrashHole = false;
            bool shouldPlaceInTower = false;
            Vector3 towerPlacementPosition = Vector3.zero;

            if (m_CurrentDraggedObject != null)
            {
                RectTransform blockRect = m_CurrentBlockView.GetRectTransform();
                
                if (blockRect != null && m_TrashHoleController.IsBlockTouchingHole(blockRect))
                {
                    isInTrashHole = true;
                    shouldDestroy = true;
                    Debug.Log("DragAndDropSystem: Block is touching trash hole - will be destroyed");
                }
                else
                {
                    Vector3 correctedEndPosition = new Vector3(endPosition.x, endPosition.y, 0f);

                    if (m_TowerController.IsBlockInTowerArea(m_CurrentBlockView, correctedEndPosition))
                    {
                        shouldPlaceInTower = true;
                        towerPlacementPosition = correctedEndPosition;
                        Debug.Log("DragAndDropSystem: Block can be placed in tower - waiting for animation");
                    }
                }
            }
            
            var animation = GetOrCreateAnimation(m_CurrentBlockView);
            
            if (shouldDestroy && isInTrashHole)
            {
                animation.ScaleDown(() => {
                    HandleTrashDestruction();
                });
            }
            else if (shouldPlaceInTower)
            {
                animation.ScaleDown(() => {
                    OnAnimationCompleteForTowerPlacement(towerPlacementPosition);
                });
                Debug.Log("DragAndDropSystem: Started scale down animation before tower placement");
            }
            else
            {
                animation.ScaleDown(() => {
                    HandleRegularDestruction();
                });
                Debug.Log("DragAndDropSystem: Block will be destroyed with animation");
            }

            Debug.Log("DragAndDropSystem: Finished dragging");
        }

        private void HandleTrashDestruction()
        {
            if (m_CurrentBlockView != null)
            {
                m_TrashHoleController.DestroyBlockInHole(m_CurrentDraggedObject);
        
                CleanupAnimation(m_CurrentBlockView);
                PopupTextController.ShowPopupText(TextBlockConstants.BlockTrashedText);
            }
    
            ScrollEvents.RequestUnblockScroll();
            ResetDragState();
        }

        private void HandleRegularDestruction()
        {
            if (m_CurrentBlockView != null)
            {
                var draggable = m_CurrentBlockView.GetDraggableBlockController();
                draggable.SetDragType(DragType.Destroying);

                PopupTextController.ShowPopupText(TextBlockConstants.BlockDestroyedText);

                var destructionAnimation = GetOrCreateDestructionAnimation(m_CurrentBlockView);
                destructionAnimation.StartDestruction(() =>
                {
                    if (m_CurrentDraggedObject != null)
                    {
                        Object.Destroy(m_CurrentDraggedObject);
                    }

                    CleanupAnimation(m_CurrentBlockView);
                    CleanupDestructionAnimation(m_CurrentBlockView);
                    Debug.Log("DragAndDropSystem: Block destroyed after animation");
            
                    ScrollEvents.RequestUnblockScroll();
                    ResetDragState();
                });

                Debug.Log("DragAndDropSystem: Block destruction animation started");
            }
            else
            {
                ScrollEvents.RequestUnblockScroll();
                ResetDragState();
            }
        }
        
        private void OnAnimationCompleteForTowerPlacement(Vector3 towerPlacementPosition)
        {
            if (m_CurrentBlockView != null && m_TowerController.TryPlaceBlockInTower(m_CurrentBlockView, towerPlacementPosition))
            {
                CleanupAnimation(m_CurrentBlockView);
                PopupTextController.ShowPopupText(TextBlockConstants.BlockPlacedText);
        
                m_CurrentBlockView.gameObject.SetActive(true);
        
                var canvasRenderer = m_CurrentBlockView.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetAlpha(1.0f);
                }
        
                Canvas.ForceUpdateCanvases();
                
                m_TowerController.SaveTowerState();

                Debug.Log($"DragAndDropSystem: Block {m_CurrentBlockView.GetColorName()} placed in tower after animation completion");
            }
            else
            {
                Debug.LogWarning("DragAndDropSystem: Failed to place block in tower after animation - destroying");

                if (m_CurrentDraggedObject != null)
                {
                    Object.Destroy(m_CurrentDraggedObject);
                }

                CleanupAnimation(m_CurrentBlockView);
                PopupTextController.ShowPopupText(TextBlockConstants.BlockDestroyedText);
            }
    
            ScrollEvents.RequestUnblockScroll();
            ResetDragState();
        }

        private void UnsubscribeFromInputEvents()
        {
            InputManager.OnMouseDown -= OnMouseDown;
            InputManager.OnMouseUp -= OnMouseUp;
            InputManager.OnStartDrag -= OnStartDrag;
            InputManager.OnDragging -= OnDragging;
            InputManager.OnEndDrag -= OnEndDrag;
        }

        public void Dispose()
        {
            UnsubscribeFromInputEvents();

            foreach (var animation in m_BlockAnimations.Values)
            {
                animation.Dispose();
            }

            foreach (var animation in m_DestructionAnimations.Values)
            {
                animation.Dispose();
            }

            m_BlockAnimations.Clear();
            m_DestructionAnimations.Clear();
        }
    }
}