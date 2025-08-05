using System;
using System.Collections.Generic;
using Core.Animations.External.PopupText.External;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime;
using Core.DragAndDrop.Runtime;
using Core.InputSystem.External;
using Core.Localization.Runtime;
using Core.Tower.Runtime;
using Core.TrashHole.Runtime;
using Core.UI.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.SimpleTimerSystem.Runtime;
using Zenject;

namespace Core.DragAndDrop.External
{
    public class DragAndDropController : IInitializable, IDisposable
{
    [Inject] private ICoreUIController m_CoreUIController;
    [Inject] private ITimerController m_TimerController;
    [Inject] private ITrashHoleController m_TrashHoleController;
    [Inject] private ITowerController m_TowerController;
    [Inject] private IBlockFactoryController m_BlockFactoryController;

    private DragState m_DragState;
    private IDragValidator m_DragValidator;
    private IAnimationManager m_AnimationManager;
    private IDragResultHandler m_DragResultHandler;
    private IDragResultResolver m_DragResultResolver;
    private PickupTimer m_PickupTimer;
    private Camera m_MainCamera;

    private const float m_LerpForce = 1000f;

    public void Initialize()
    {
        Debug.Log("DragAndDropSystem: Initialized");

        InitializeDependencies();
        SubscribeToInputEvents();
        SetupTimer();
    }

    private void InitializeDependencies()
    {
        m_DragState = new DragState();
        m_AnimationManager = new AnimationManager();
        
        var animationChecker = new AnimationChecker(
            m_AnimationManager.GetPickupAnimations(),
            m_AnimationManager.GetDestructionAnimations()
        );
        
        m_DragValidator = new DragValidator(m_TowerController, animationChecker);
        m_DragResultHandler = new DragResultHandler(m_TrashHoleController, m_TowerController, m_AnimationManager);
        m_DragResultResolver = new DragResultResolver(m_TrashHoleController, m_TowerController);
    }

    private void SetupTimer()
    {
        m_PickupTimer = new PickupTimer(m_TimerController);
        m_PickupTimer.OnTimerComplete += OnPickupTimerComplete;
    }

    private void OnPickupTimerComplete()
    {
        if (m_DragState.m_IsWaitingForPickup && m_DragState.m_CurrentBlockView != null)
        {
            ScrollEvents.RequestBlockScroll();
            PickUpBlock(m_DragState.m_CurrentBlockView);
            m_DragState.m_IsWaitingForPickup = false;
            Debug.Log("DragAndDropSystem: Timer completed - block picked up and scroll blocked");
        }
    }

    private void OnMouseDown(Vector3 worldPosition)
    {
        var blockView = FindDraggableAtPosition(worldPosition);
        if (blockView == null || !m_DragValidator.CanStartDrag(blockView))
        {
            return;
        }

        HandlePreviousBlock(blockView);
        
        var animation = m_AnimationManager.GetOrCreatePickupAnimation(blockView);
        if (animation.IsAnimating() || !animation.IsAtReferenceScale())
        {
            Debug.Log("DragAndDropSystem: Animation in progress or not at reference scale - ignoring click");
            return;
        }

        // Запоминаем оригинальный блок
        m_DragState.m_OriginalBlockView = blockView;
        m_DragState.m_CurrentBlockView = blockView; // Пока что тот же
        
        var dragBehavior = blockView.GetDraggableBlockController().GetDragBehavior();

        if (dragBehavior == DragType.Move)
        {
            ScrollEvents.RequestBlockScroll();
            PickUpBlock(blockView);
            Debug.Log("DragAndDropSystem: Move block picked up immediately");
        }
        else if (dragBehavior == DragType.Clone)
        {
            m_PickupTimer.StartTimer();
            m_DragState.m_IsWaitingForPickup = true;
            m_DragState.m_IsCloneOperation = true;
            Debug.Log("DragAndDropSystem: Clone block - timer started for pickup");
        }
    }

    private void HandlePreviousBlock(BlockView newBlockView)
    {
        if (m_DragState.m_CurrentBlockView != null && m_DragState.m_CurrentBlockView != newBlockView)
        {
            // Сбрасываем анимацию предыдущего блока
            m_AnimationManager.ForceResetScale(m_DragState.m_CurrentBlockView);
        }
    }

    private void OnMouseUp(Vector3 worldPosition)
    {
        if (m_DragState.m_IsWaitingForPickup)
        {
            StopWaitingForPickup();
            Debug.Log("DragAndDropSystem: Mouse up - timer stopped");
            return;
        }

        if (m_DragState.m_HasPickedUpBlock && !m_DragState.m_IsDragging && m_DragState.m_CurrentBlockView != null)
        {
            var currentPosition = m_DragState.m_CurrentBlockView.transform.position;
            FinishDragging(currentPosition);
            Debug.Log("DragAndDropSystem: Mouse up without drag - treating as finished drag");
        }
    }

    private void StopWaitingForPickup()
    {
        m_PickupTimer.StopTimer();
        m_DragState.m_IsWaitingForPickup = false;
        ScrollEvents.RequestUnblockScroll();
        ResetDragState();
    }

    private void OnStartDrag(Vector3 worldPosition)
    {
        if (m_DragState.m_IsWaitingForPickup)
        {
            StopWaitingForPickup();
            Debug.Log("DragAndDropSystem: Drag started before timer - timer stopped");
            return;
        }

        if (m_DragState.m_CurrentDraggedObject != null)
        {
            m_DragState.m_IsDragging = true;
            Debug.Log("DragAndDropSystem: Drag actually started");
        }
    }

    private void OnDragging(Vector3 worldPosition)
    {
        if (m_DragState.m_CurrentDraggedObject != null && m_DragState.m_IsDragging)
        {
            UpdateDraggedObjectPosition(worldPosition);
        }
    }

    private void OnEndDrag(Vector3 worldPosition)
    {
        if (m_DragState.m_IsWaitingForPickup)
        {
            StopWaitingForPickup();
            Debug.Log("DragAndDropSystem: Mouse released - timer stopped");
            return;
        }

        if (m_DragState.m_CurrentDraggedObject != null && m_DragState.m_IsDragging && m_DragState.m_CurrentBlockView != null)
        {
            FinishDragging(worldPosition);
        }
        else
        {
            ScrollEvents.RequestUnblockScroll();
            ResetDragState();
        }
    }

    private void FinishDragging(Vector3 endPosition)
    {
        if (m_DragState.m_CurrentBlockView == null || m_DragState.m_CurrentBlockView.GetDraggableBlockController() == null)
        {
            Debug.LogWarning("DragAndDropSystem: CurrentBlockView or DraggableBlockController is null");
            ScrollEvents.RequestUnblockScroll();
            ResetDragState();
            return;
        }

        var result = m_DragResultResolver.ResolveDragResult(
            m_DragState.m_CurrentBlockView, 
            m_DragState.m_CurrentDraggedObject, 
            endPosition
        );
        
        result.OnComplete = ResetDragState;
        m_DragResultHandler.HandleDragResult(result);
        Debug.Log("DragAndDropSystem: Finished dragging");
    }

    private void PickUpBlock(BlockView blockView)
    {
        var dragBehavior = blockView.GetDraggableBlockController().GetDragBehavior();

        if (dragBehavior == DragType.Clone || m_DragState.m_IsCloneOperation)
        {
            // Создаем клонированный блок
            CreateDraggedObject(blockView);
            // Теперь m_CurrentBlockView указывает на клонированный блок
            // А m_OriginalBlockView остается указывать на оригинальный
        }
        else if (dragBehavior == DragType.Move)
        {
            m_TowerController.RemoveBlockFromTower(blockView);
            m_TowerController.SaveTowerState();
            m_DragState.m_CurrentDraggedObject = blockView.GetDraggableBlockController().GetGameObject();
            // При Move и оригинальный, и текущий - один и тот же блок
        }

        if (m_DragState.m_CurrentDraggedObject != null)
        {
            m_DragState.m_CurrentDraggedObject.transform.SetAsLastSibling();
        }

        // Анимируем ВСЕГДА текущий блок (тот который мы перемещаем)
        var animation = m_AnimationManager.GetOrCreatePickupAnimation(m_DragState.m_CurrentBlockView);
        animation.ScaleUp();

        m_DragState.m_HasPickedUpBlock = true;
        m_DragState.m_CurrentBlockView.GetDraggableBlockController().OnDragStart();
        PopupTextController.ShowPopupText(LocalizationKeys.BlockPickedUpTextKey.Localize());
    }

    private void CreateDraggedObject(BlockView blockView)
    {
        var originalId = blockView.GetId();
        var draggedBlockView = m_BlockFactoryController.CreateBlockById(
            originalId,
            m_CoreUIController.GetCoreView().DraggingBlockView,
            DragType.Move
        );

        if (draggedBlockView != null)
        {
            var originalRect = blockView.GetRectTransform();
            draggedBlockView.transform.position = originalRect.position;
            draggedBlockView.transform.rotation = originalRect.rotation;

            m_DragState.m_CurrentDraggedObject = draggedBlockView.gameObject;
            m_DragState.m_CurrentBlockView = draggedBlockView;

            Debug.Log($"DragAndDropSystem: Created cloned object with ID {originalId} and DragType.Move");
        }
        else
        {
            Debug.LogError($"DragAndDropSystem: Failed to create cloned block with ID {originalId}");
        }
    }

    private void UpdateDraggedObjectPosition(Vector3 worldPosition)
    {
        if (m_DragState.m_CurrentDraggedObject == null) return;

        var followedPosition = m_DragState.m_CurrentDraggedObject.transform.position;
        var dragPosition = new Vector3(worldPosition.x, worldPosition.y, followedPosition.z);
        
        m_DragState.m_CurrentDraggedObject.transform.position = Vector3.Lerp(
            followedPosition, 
            dragPosition, 
            Time.deltaTime * m_LerpForce
        );
    }

    private void ResetDragState()
    {
        // Сбрасываем анимацию текущего блока (того что мы перемещали)
        if (m_DragState.m_CurrentBlockView != null)
        {
            m_AnimationManager.ForceResetScale(m_DragState.m_CurrentBlockView);
        }
        
        // Если была операция клонирования, также сбрасываем оригинальный блок
        if (m_DragState.m_IsCloneOperation && m_DragState.m_OriginalBlockView != null && 
            m_DragState.m_OriginalBlockView != m_DragState.m_CurrentBlockView)
        {
            m_AnimationManager.ForceResetScale(m_DragState.m_OriginalBlockView);
        }
        
        m_DragState.Reset();
    }

    private BlockView FindDraggableAtPosition(Vector3 worldPosition)
    {
        var pointerData = new PointerEventData(EventSystem.current);
        var resultsData = new List<RaycastResult>();
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

    private void SubscribeToInputEvents()
    {
        InputManager.OnMouseDown += OnMouseDown;
        InputManager.OnMouseUp += OnMouseUp;
        InputManager.OnStartDrag += OnStartDrag;
        InputManager.OnDragging += OnDragging;
        InputManager.OnEndDrag += OnEndDrag;
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
        
        if (m_PickupTimer != null)
        {
            m_PickupTimer.OnTimerComplete -= OnPickupTimerComplete;
        }
        
        m_AnimationManager?.Dispose();
    }
}
}