using DG.Tweening;
using UnityEngine;
using System;

namespace Core.Animations.External
{
    public class DestructionAnimation
    {
        private RectTransform m_RectTransform;
        private Vector3 m_OriginalScale;
        private bool m_IsAnimating = false;
        
        private const float m_ExplosionScaleMultiplier = 1.5f;
        private const float m_ExplosionDuration = 0.15f;
        private const float m_ShrinkDuration = 0.25f;
        private const Ease m_ExplosionEase = Ease.OutBack;
        private const Ease m_ShrinkEase = Ease.InBack;
        
        private Sequence m_DestructionSequence;
        
        public void Initialize(RectTransform rectTransform)
        {
            m_RectTransform = rectTransform;
            
            if (m_RectTransform != null)
            {
                m_OriginalScale = m_RectTransform.localScale;
                Debug.Log($"DestructionAnimation: Initialized with scale {m_OriginalScale}");
            }
        }
        
        public void StartDestruction(Action onComplete = null)
        {
            if (m_RectTransform == null || m_IsAnimating)
                return;
                
            m_IsAnimating = true;
            
            StopCurrentAnimation();
            
            Vector3 explosionScale = m_OriginalScale * m_ExplosionScaleMultiplier;
            
            m_DestructionSequence = DOTween.Sequence();
            
            m_DestructionSequence.Append(
                m_RectTransform.DOScale(explosionScale, m_ExplosionDuration)
                    .SetEase(m_ExplosionEase)
            );
            
            m_DestructionSequence.Append(
                m_RectTransform.DOScale(Vector3.zero, m_ShrinkDuration)
                    .SetEase(m_ShrinkEase)
            );
            
            m_DestructionSequence.OnComplete(() =>
            {
                m_IsAnimating = false;
                onComplete?.Invoke();
                Debug.Log("DestructionAnimation: Destruction animation completed");
            });
            
            Debug.Log("DestructionAnimation: Started destruction animation");
        }
        
        public void ResetToOriginal()
        {
            if (m_RectTransform == null)
                return;
                
            StopCurrentAnimation();
            
            m_RectTransform.localScale = m_OriginalScale;
            m_RectTransform.rotation = Quaternion.identity;
            m_IsAnimating = false;
            
            Debug.Log("DestructionAnimation: Reset to original state");
        }
        
        public void UpdateOriginalScale()
        {
            if (m_RectTransform == null)
                return;
                
            m_OriginalScale = m_RectTransform.localScale;
            
            Debug.Log($"DestructionAnimation: Updated original scale: {m_OriginalScale}");
        }
        
        private void StopCurrentAnimation()
        {
            m_DestructionSequence?.Kill();
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