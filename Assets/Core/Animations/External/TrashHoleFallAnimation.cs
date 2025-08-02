using System;
using DG.Tweening;
using UnityEngine;

namespace Core.Animations.External
{
    public class TrashHoleFallAnimation
    {
        private GameObject m_AnimatedObject;
        private Canvas m_Canvas;
        private Vector3 m_StartPosition;
        private Vector3 m_TargetPosition;
        private Action m_OnCompleteCallback;
        
        private Sequence m_AnimationSequence;
        
        private const float m_AnimationDuration = 0.6f;
        private const float m_RotationSpeed = 180f;
        private const float m_ScaleDownFactor = 0.6f;
        
        public void StartFallAnimation(GameObject objectToAnimate, Vector3 holeCenter, Canvas canvas, Action onComplete = null)
        {
            if (objectToAnimate == null)
            {
                Debug.LogError("TrashHoleFallAnimation: Object to animate is null!");
                onComplete?.Invoke();
                return;
            }
            
            if (canvas == null)
            {
                Debug.LogError("TrashHoleFallAnimation: Canvas is null!");
                onComplete?.Invoke();
                return;
            }
            
            m_AnimatedObject = objectToAnimate;
            m_Canvas = canvas;
            m_StartPosition = objectToAnimate.transform.position;
            m_OnCompleteCallback = onComplete;
            
            m_TargetPosition = CalculateHoleBottomCenter(holeCenter);
            
            Debug.Log($"TrashHoleFallAnimation: Start: {m_StartPosition}, Target: {m_TargetPosition}, Hole: {holeCenter}");
            
            StartAnimation();
        }
        
        private void StartAnimation()
        {
            StopAnimation();
            
            m_AnimationSequence = DOTween.Sequence();
            
            AnimateTrajectory();
            AnimateRotation();
            AnimateScale();
            
            m_AnimationSequence.OnComplete(() =>
            {
                OnAnimationComplete();
            });
            
            Debug.Log($"TrashHoleFallAnimation: Started fall animation for {m_AnimatedObject.name}");
        }
        
        private void AnimateTrajectory()
        {
            Vector3[] waypoints = CalculateControlledArcWaypoints();
            
            m_AnimationSequence.Join(
                m_AnimatedObject.transform.DOPath(waypoints, m_AnimationDuration, PathType.CatmullRom)
                    .SetEase(Ease.InOutQuad) 
            );
        }
        
        private Vector3[] CalculateControlledArcWaypoints()
        {
            float safeJumpHeight = CalculateSafeJumpHeight();
            
            Vector3 startToTarget = m_TargetPosition - m_StartPosition;
            float distance = startToTarget.magnitude;
            
            Vector3 firstMid = m_StartPosition + startToTarget * 0.25f;
            firstMid.y += safeJumpHeight * 0.6f;
            
            Vector3 peak = m_StartPosition + startToTarget * 0.5f;
            peak.y += safeJumpHeight;
            
            Vector3 secondMid = m_StartPosition + startToTarget * 0.75f;
            secondMid.y += safeJumpHeight * 0.3f;
            
            return new Vector3[]
            {
                m_StartPosition,
                firstMid,
                peak,
                secondMid,
                m_TargetPosition
            };
        }
        
        private Vector3 CalculateHoleBottomCenter(Vector3 holeCenter)
        {
            return new Vector3(holeCenter.x, holeCenter.y, holeCenter.z);
        }

        private float CalculateSafeJumpHeight()
        {
            if (m_Canvas == null)
            {
                return 20f;
            }
            
            RectTransform canvasRect = m_Canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
            {
                return 20f;
            }
            
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);
            
            float canvasRightX = canvasCorners[2].x;
            float canvasLeftX = canvasCorners[0].x;
            float canvasWidth = canvasRightX - canvasLeftX;
            
            float arcHeight = canvasWidth * 0.15f;
            
            Debug.Log($"TrashHoleFallAnimation: Canvas width: {canvasWidth}, Arc height: {arcHeight}");
            
            return arcHeight;
        }
        
        private void AnimateRotation()
        {
            float totalRotation = m_RotationSpeed * m_AnimationDuration;
            
            m_AnimationSequence.Join(
                m_AnimatedObject.transform.DORotate(
                    new Vector3(0, 0, totalRotation), 
                    m_AnimationDuration, 
                    RotateMode.LocalAxisAdd
                ).SetEase(Ease.Linear)
            );
        }
        
        private void AnimateScale()
        {
            Vector3 originalScale = m_AnimatedObject.transform.localScale;
            Vector3 targetScale = originalScale * m_ScaleDownFactor;
            
            m_AnimationSequence.Insert(
                m_AnimationDuration * 0.6f,
                m_AnimatedObject.transform.DOScale(targetScale, m_AnimationDuration * 0.4f)
                    .SetEase(Ease.InQuad)
            );
        }
        
        private void OnAnimationComplete()
        {
            Debug.Log($"TrashHoleFallAnimation: Animation completed for {m_AnimatedObject?.name}");
            
            m_OnCompleteCallback?.Invoke();
            
            Cleanup();
        }
        
        public void StopAnimation()
        {
            if (m_AnimationSequence != null && m_AnimationSequence.IsActive())
            {
                m_AnimationSequence.Kill();
                m_AnimationSequence = null;
            }
        }
        
        public bool IsAnimating()
        {
            return m_AnimationSequence != null && m_AnimationSequence.IsActive();
        }
        
        private void Cleanup()
        {
            m_AnimatedObject = null;
            m_Canvas = null;
            m_OnCompleteCallback = null;
            m_AnimationSequence = null;
        }
        
        public void Dispose()
        {
            StopAnimation();
            Cleanup();
        }
    }
}