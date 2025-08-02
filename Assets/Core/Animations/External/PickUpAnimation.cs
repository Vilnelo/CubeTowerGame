using DG.Tweening;
using UnityEngine;

namespace Core.Animations.External
{
    public class PickUpAnimation
    {
        private RectTransform m_RectTransform;
        private Vector3 m_OriginalScale;
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
                m_OriginalScale = m_RectTransform.localScale;
                Debug.Log($"PickUpAnimation: Initialized with scale {m_OriginalScale}");
            }
        }
        
        public void ScaleUp()
        {
            if (m_RectTransform == null || m_IsAnimating)
                return;
                
            m_IsAnimating = true;
            
            StopCurrentAnimation();
            
            Vector3 targetScale = m_OriginalScale * m_PickupScaleMultiplier;
            
            m_ScaleTween = m_RectTransform.DOScale(targetScale, m_AnimationDuration)
                .SetEase(m_PickupEase)
                .OnComplete(() => m_IsAnimating = false);
            
            Debug.Log("PickUpAnimation: Started scale up animation");
        }
        
        public void ScaleDown()
        {
            if (m_RectTransform == null || m_IsAnimating)
                return;
                
            m_IsAnimating = true;
            
            StopCurrentAnimation();
            
            m_ScaleTween = m_RectTransform.DOScale(m_OriginalScale, m_AnimationDuration)
                .SetEase(m_ReturnEase)
                .OnComplete(() => m_IsAnimating = false);
            
            Debug.Log("PickUpAnimation: Started scale down animation");
        }
        
        public void ResetScale()
        {
            if (m_RectTransform == null)
                return;
                
            StopCurrentAnimation();
            
            m_RectTransform.localScale = m_OriginalScale;
            m_IsAnimating = false;
            
            Debug.Log("PickUpAnimation: Reset to original scale");
        }
        
        public void UpdateOriginalScale()
        {
            if (m_RectTransform == null)
                return;
                
            m_OriginalScale = m_RectTransform.localScale;
            
            Debug.Log($"PickUpAnimation: Updated original scale: {m_OriginalScale}");
        }
        
        private void StopCurrentAnimation()
        {
            m_ScaleTween?.Kill();
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