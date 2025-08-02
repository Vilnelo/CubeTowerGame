using System;
using System.Collections.Generic;
using Core.Animations.External;
using Core.AssetManagement.Runtime;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime;
using Core.Canvases.External;
using Core.Canvases.Runtime;
using Core.DragAndDrop.Runtime;
using Core.InputSystem.External;
using Core.TrashHole.Runtime;
using Core.UI.External;
using Core.UI.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.SimpleTimerSystem;
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

        private GameObject m_CurrentDraggedObject;
        private Camera m_MainCamera;
        private SimpleTimerWrapper m_CountdownTimer;
        private BlockView m_CurrentBlockView;
        private bool m_IsWaitingForPickup = false;
        private bool m_IsDragging = false;
        private bool m_HasPickedUpBlock = false;

        private Dictionary<RectTransform, PickUpAnimation> m_BlockAnimations =
            new Dictionary<RectTransform, PickUpAnimation>();

        private const float m_LerpForce = 10f;
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

            m_CurrentBlockView = blockView;
            var draggable = blockView.GetDraggableBlockController();
            var dragBehavior = draggable.GetDragBehavior();

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
                var animation = GetOrCreateAnimation(m_CurrentBlockView);

                if (!animation.IsAtReferenceScale())
                {
                    animation.ScaleDown();
                }

                ScrollEvents.RequestUnblockScroll();
                ResetDragState();
                Debug.Log("DragAndDropSystem: Mouse up without drag - scaling down");
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
                m_CurrentDraggedObject = m_CurrentBlockView.GetDraggableBlockController().GetGameObject();
            }

            m_CurrentDraggedObject.transform.SetAsLastSibling();

            var animation = GetOrCreateAnimation(m_CurrentBlockView);
            animation.ScaleUp();

            m_HasPickedUpBlock = true;

            m_CurrentBlockView.GetDraggableBlockController().OnDragStart();
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
                Debug.Log("DragAndDropSystem: Mouse released - timer stopped");
            }

            if (m_CurrentDraggedObject != null && m_IsDragging && m_CurrentBlockView != null)
            {
                FinishDragging(worldPosition);

                var animation = GetOrCreateAnimation(m_CurrentBlockView);
                animation.ScaleDown();
            }

            ScrollEvents.RequestUnblockScroll();
            ResetDragState();
        }

        private void ResetDragState()
        {
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

            var draggedBlockView = Object.Instantiate(originalBlock.GetBlockPrefab(),
                originalBlock.GetRectTransform().position,
                originalBlock.GetRectTransform().rotation,
                m_CoreUIController.GetCoreView().DraggingBlockView);

            if (draggedBlockView != null)
            {
                m_CurrentDraggedObject = draggedBlockView;

                if (!draggedBlockView.TryGetComponent<BlockView>(out var newBlockView))
                {
                    Debug.LogError("DragAndDropSystem: cannot find DraggableBlockView");
                    return;
                }

                m_CurrentBlockView = newBlockView;

                Debug.Log("DragAndDropSystem: Created dragged object");
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
            if (m_CurrentBlockView.GetDraggableBlockController() != null)
            {
                bool shouldDestroy = false;
                if (m_CurrentDraggedObject != null)
                {
                    RectTransform blockRect = m_CurrentBlockView.GetRectTransform();
                    if (blockRect != null && m_TrashHoleController.IsBlockTouchingHole(blockRect))
                    {
                        shouldDestroy = true;
                        Debug.Log("DragAndDropSystem: Block is touching trash hole - will be destroyed");
                    }
                }

                if (shouldDestroy)
                {
                    m_TrashHoleController.DestroyBlockInHole(m_CurrentDraggedObject);
                    CleanupAnimation(m_CurrentBlockView);
                }
                else
                {
                    m_CurrentBlockView.GetDraggableBlockController().OnDragEnd(endPosition);
                }
            }

            Debug.Log("DragAndDropSystem: Finished dragging");
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

            m_BlockAnimations.Clear();
        }
    }
}