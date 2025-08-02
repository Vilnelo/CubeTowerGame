using DG.Tweening;
using UnityEngine;

namespace Core.Animations.External
{
    public class PickUpAnimation
    {
        private RectTransform m_RectTransform;
        private Vector3 m_ReferenceScale;
        private Vector3 m_TargetUpScale;
        private bool m_IsAnimating = false;

        private const float m_PickupScaleMultiplier = 1.15f;
        private const float m_AnimationDuration = 0.2f;
        private const Ease m_PickupEase = Ease.OutBack;
        private const Ease m_ReturnEase = Ease.OutQuad;

        private Tween m_ScaleTween;

        public void Initialize(RectTransform rectTransform)
        {
            m_RectTransform = rectTransform;

            if (m_RectTransform != null)
            {
                m_ReferenceScale = m_RectTransform.localScale;
                m_TargetUpScale = m_ReferenceScale * m_PickupScaleMultiplier;

                m_RectTransform.localScale = m_ReferenceScale;

                Debug.Log($"PickUpAnimation: Initialized with reference scale {m_ReferenceScale}");
            }
        }

        public void ScaleUp()
        {
            if (m_RectTransform == null)
                return;

            StopCurrentAnimation();

            m_RectTransform.localScale = m_ReferenceScale;

            m_IsAnimating = true;

            m_ScaleTween = m_RectTransform.DOScale(m_TargetUpScale, m_AnimationDuration)
                .SetEase(m_PickupEase)
                .OnComplete(() =>
                {
                    m_IsAnimating = false;
                    Debug.Log("PickUpAnimation: Scale up completed");
                });

            Debug.Log("PickUpAnimation: Started scale up animation");
        }

        public void ScaleDown()
        {
            if (m_RectTransform == null)
                return;

            StopCurrentAnimation();

            m_IsAnimating = true;

            m_ScaleTween = m_RectTransform.DOScale(m_ReferenceScale, m_AnimationDuration)
                .SetEase(m_ReturnEase)
                .OnComplete(() =>
                {
                    m_IsAnimating = false;

                    if (m_RectTransform != null)
                    {
                        m_RectTransform.localScale = m_ReferenceScale;
                    }

                    Debug.Log("PickUpAnimation: Scale down completed");
                });

            Debug.Log("PickUpAnimation: Started scale down animation");
        }

        public void ForceResetToReferenceScale()
        {
            if (m_RectTransform == null)
                return;

            StopCurrentAnimation();

            m_RectTransform.localScale = m_ReferenceScale;
            m_IsAnimating = false;

            Debug.Log($"PickUpAnimation: Force reset to reference scale {m_ReferenceScale}");
        }

        public bool IsAtReferenceScale()
        {
            if (m_RectTransform == null)
                return false;

            return Vector3.Distance(m_RectTransform.localScale, m_ReferenceScale) < 0.01f;
        }

        private void StopCurrentAnimation()
        {
            if (m_ScaleTween != null && m_ScaleTween.IsActive())
            {
                m_ScaleTween.Kill();
                Debug.Log("PickUpAnimation: Stopped current animation");
            }

            m_IsAnimating = false;
        }

        public bool IsAnimating()
        {
            return m_IsAnimating;
        }

        public void Dispose()
        {
            StopCurrentAnimation();
            m_RectTransform = null;
        }
    }
}