using System;
using DG.Tweening;
using UnityEngine;

namespace Core.Animations.External
{
    public class TowerJumpAnimation
    {
        private GameObject m_AnimatedObject;
        private Canvas m_Canvas;
        private Vector3 m_StartPosition;
        private Vector3 m_TargetPosition;
        private Action m_OnCompleteCallback;
        private Sequence m_AnimationSequence;

        private const float m_AnimationDuration = 0.4f;

        public void StartJumpAnimation(GameObject objectToAnimate, Vector3 targetPosition, Canvas canvas, Action onComplete = null)
        {
            if (objectToAnimate == null)
            {
                Debug.LogError("TowerJumpAnimation: Object to animate is null!");
                onComplete?.Invoke();
                return;
            }

            if (canvas == null)
            {
                Debug.LogError("TowerJumpAnimation: Canvas is null!");
                onComplete?.Invoke();
                return;
            }

            m_AnimatedObject = objectToAnimate;
            m_Canvas = canvas;
            m_StartPosition = objectToAnimate.transform.position;
            m_TargetPosition = targetPosition;
            m_OnCompleteCallback = onComplete;

            Debug.Log($"TowerJumpAnimation: Start: {m_StartPosition}, Target: {m_TargetPosition}");

            StartAnimation();
        }

        private void StartAnimation()
        {
            StopAnimation();

            m_AnimationSequence = DOTween.Sequence();

            AnimateJumpTrajectory();

            m_AnimationSequence.OnComplete(() => { OnAnimationComplete(); });

            Debug.Log($"TowerJumpAnimation: Started jump animation for {m_AnimatedObject.name}");
        }

        private void AnimateJumpTrajectory()
        {
            Vector3[] waypoints = CalculateJumpWaypoints();

            m_AnimationSequence.Join(
                m_AnimatedObject.transform.DOPath(waypoints, m_AnimationDuration, PathType.CatmullRom)
                    .SetEase(Ease.OutQuad)
            );
        }

        private Vector3[] CalculateJumpWaypoints()
        {
            float jumpHeight = CalculateSafeJumpHeight();

            Vector3 startToTarget = m_TargetPosition - m_StartPosition;
            Vector3 peak = m_StartPosition + startToTarget * 0.5f;
            peak.y += jumpHeight;

            Debug.Log($"TowerJumpAnimation: Jump height: {jumpHeight}, Peak: {peak}");

            return new Vector3[]
            {
                m_StartPosition,
                peak,
                m_TargetPosition
            };
        }

        private float CalculateSafeJumpHeight()
        {
            if (m_Canvas == null)
            {
                Debug.LogWarning("TowerJumpAnimation: Canvas is null, using default jump height");
                return 50f;
            }

            RectTransform canvasRect = m_Canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
            {
                Debug.LogWarning("TowerJumpAnimation: Canvas RectTransform is null, using default jump height");
                return 50f;
            }

            // Получаем границы канваса
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            float canvasTop = canvasCorners[2].y; // Верхний край канваса
            float canvasBottom = canvasCorners[0].y; // Нижний край канваса
            float canvasHeight = canvasTop - canvasBottom;

            // Находим самую высокую Y координату между стартом и целью
            float highestBlockY = Mathf.Max(m_StartPosition.y, m_TargetPosition.y);
            
            // Высота прыжка = середина между верхним блоком и верхом экрана
            float jumpHeight = (canvasTop - highestBlockY) * 0.5f;
            
            // Ограничиваем минимальную и максимальную высоту прыжка
            jumpHeight = Mathf.Clamp(jumpHeight, canvasHeight * 0.1f, canvasHeight * 0.3f);

            Debug.Log($"TowerJumpAnimation: Canvas height: {canvasHeight}, Highest block Y: {highestBlockY}, Jump height: {jumpHeight}");

            return jumpHeight;
        }

        private void OnAnimationComplete()
        {
            Debug.Log($"TowerJumpAnimation: Animation completed for {m_AnimatedObject?.name}");
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