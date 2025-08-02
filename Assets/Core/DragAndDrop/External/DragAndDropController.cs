using System;
using System.Collections.Generic;
using Core.AssetManagement.Runtime;
using Core.BottomBlocks.External;
using Core.Canvases.External;
using Core.Canvases.Runtime;
using Core.DragAndDrop.Runtime;
using Core.InputSystem.External;
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
        [Inject] private IMainCanvas m_Canvas;
        [Inject] private ITimerController m_TimerController;

        private GameObject m_CurrentDraggedObject;
        private IDraggable m_CurrentDraggable;
        private Camera m_MainCamera;
        private SimpleTimerWrapper m_CountdownTimer;
        
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

        private void StartPickUpTimer()
        {
            m_CountdownTimer.StopTimer();
            m_CountdownTimer.ResetTimer(); 
            m_CountdownTimer.StartTimer();
        }
        
        private void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            //Do nothing
        }
    
        private void OnTimerComplete(SimpleTimerInfo timerInfo)
        {
            m_CountdownTimer.StopTimer();
            Debug.Log("Timer completed!");
        }
        
        private void OnMouseDown(Vector3 worldPosition)
        {
            var bottomBlockView = FindDraggableAtPosition(worldPosition);

            if (bottomBlockView == null)
            {
                return;
            }
            
            StartPickUpTimer();
        }
        
        private void OnStartDrag(Vector3 worldPosition)
        {
            var bottomBlockView = FindDraggableAtPosition(worldPosition);
            if (bottomBlockView != null)
            {
                StartDragging(bottomBlockView, worldPosition);
            }
        }
        
        private void OnDragging(Vector3 worldPosition)
        {
            if (m_CurrentDraggedObject != null)
            {
                UpdateDraggedObjectPosition(worldPosition);
            }
        }
        
        private void OnEndDrag(Vector3 worldPosition)
        {
            if (m_CurrentDraggedObject != null)
            {
                FinishDragging(worldPosition);
            }
            
            m_CountdownTimer.StopTimer();
        }
        
        private BlockView FindDraggableAtPosition(Vector3 worldPosition)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> resultsData = new List<RaycastResult>();
            pointerData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointerData, resultsData);

            foreach (var result in resultsData)
            {
                if (result.gameObject.transform.TryGetComponent<BlockView>(out var bottomBlockView))
                {
                    return bottomBlockView;
                }
            }

            return null;
        }
        
        private void StartDragging(BlockView blockView, Vector3 startPosition)
        {
            m_CurrentDraggable = blockView.GetDraggableBlockController();
            
            var dragBehavior = m_CurrentDraggable.GetDragBehavior();
            
            if (dragBehavior == DragType.Clone)
            {
                CreateDraggedObject(blockView);
            }
            else if (dragBehavior == DragType.Move)
            {
                m_CurrentDraggedObject = m_CurrentDraggable.GetGameObject();
            }
            
            m_CurrentDraggable.OnDragStart();
        }
        
        private void CreateDraggedObject(BlockView blockView)
        {
            var originalBlock = blockView;
            if (originalBlock == null)
            {
                Debug.LogError("DragAndDropSystem: Can only drag BottomBlockView objects for now");
                return;
            }
            
            var draggedBlockView = Object.Instantiate(originalBlock.GetBlockPrefab(), 
                originalBlock.GetRectTransform().position,
                originalBlock.GetRectTransform().rotation, 
                m_Canvas.GetTransform());
            
            if (draggedBlockView != null)
            {
                m_CurrentDraggedObject = draggedBlockView;
                if (!draggedBlockView.TryGetComponent<IDraggable>(out var draggable))
                {
                    Debug.LogError("DragAndDropSystem: cannot find DraggableBlockView");
                    return;
                };
                m_CurrentDraggable = draggable;
                
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
            if (m_CurrentDraggable != null)
            {
                m_CurrentDraggable.OnDragEnd(endPosition);
                
                
                //TODO: Тут проверка на область в которой закончили движение и реакцию остального проекта
            }
            
            m_CurrentDraggedObject = null;
            m_CurrentDraggable = null;
            
            Debug.Log("DragAndDropSystem: Finished dragging");
        }
        
        private void UnsubscribeFromInputEvents()
        {
            InputManager.OnMouseDown -= OnMouseDown;
            InputManager.OnStartDrag -= OnStartDrag;
            InputManager.OnDragging -= OnDragging;
            InputManager.OnEndDrag -= OnEndDrag;
        }
        
        public void Dispose()
        {
            UnsubscribeFromInputEvents();
        }
    }
}