using DG.Tweening;
using UnityEngine;

namespace Core.Animations.External.PopupText.External
{
    public class PopupTextAnimation
    {
        private PopupTextView m_PopupTextView;
        private RectTransform m_RectTransform;
        private CanvasGroup m_CanvasGroup;
        
        private Vector2 m_StartPosition;
        private Vector2 m_EndPosition;
        private bool m_IsAnimating = false;

        private const float m_AnimationDuration = 1.5f;
        private const float m_MoveDistance = 100f;
        private const Ease m_MoveEase = Ease.OutQuart;
        private const Ease m_FadeInEase = Ease.OutQuad;
        private const Ease m_FadeOutEase = Ease.InQuad;

        private Sequence m_AnimationSequence;

        public void Initialize(PopupTextView popupTextView, RectTransform spawnParent)
        {
            if (popupTextView == null)
            {
                Debug.LogError("PopupTextAnimation: PopupTextView is null");
                return;
            }

            m_PopupTextView = popupTextView;
            m_RectTransform = m_PopupTextView.GetRectTransform();
            m_CanvasGroup = m_PopupTextView.GetCanvasGroup();

            if (m_RectTransform == null || m_CanvasGroup == null)
            {
                Debug.LogError("PopupTextAnimation: Missing required components (RectTransform or CanvasGroup)");
                return;
            }
            
            m_RectTransform.SetParent(spawnParent, false);
            
            ResetTransform();
            
            m_PopupTextView.gameObject.SetActive(false);

            Debug.Log("PopupTextAnimation: Initialized successfully");
        }

        public void PlayAnimation(string text)
        {
            if (m_RectTransform == null || m_PopupTextView == null || m_CanvasGroup == null)
            {
                Debug.LogError("PopupTextAnimation: Animation not properly initialized");
                return;
            }

            if (m_IsAnimating)
            {
                Debug.LogWarning("PopupTextAnimation: Animation already playing, stopping current animation");
                StopCurrentAnimation();
            }
            
            m_PopupTextView.SetText(text);
            
            ResetTransform();
            m_PopupTextView.gameObject.SetActive(true);
            
            m_IsAnimating = true;
            
            m_AnimationSequence = DOTween.Sequence();
            
            m_AnimationSequence.Append(
                DOTween.To(() => m_RectTransform.anchoredPosition, 
                          x => m_RectTransform.anchoredPosition = x, 
                          m_EndPosition, 
                          m_AnimationDuration)
                    .SetEase(m_MoveEase)
            );
            
            m_AnimationSequence.Join(
                m_CanvasGroup.DOFade(1f, 0.2f)
                    .SetEase(m_FadeInEase)
            );
            
            m_AnimationSequence.Append(
                m_CanvasGroup.DOFade(0f, 0.3f)
                    .SetEase(m_FadeOutEase)
            );
            
            m_AnimationSequence.OnComplete(() =>
            {
                m_IsAnimating = false;
                m_PopupTextView.gameObject.SetActive(false);
                Debug.Log($"PopupTextAnimation: Animation completed for text: {text}");
            });

            Debug.Log($"PopupTextAnimation: Started animation for text: {text}");
        }

        public void ForceStop()
        {
            StopCurrentAnimation();
            
            if (m_PopupTextView != null)
            {
                m_PopupTextView.gameObject.SetActive(false);
            }
            
            Debug.Log("PopupTextAnimation: Force stopped");
        }

        private void ResetTransform()
        {
            if (m_RectTransform == null)
                return;
            
            m_RectTransform.anchoredPosition = Vector2.zero;
            m_RectTransform.localScale = Vector3.one;
            
            m_StartPosition = Vector2.zero;
            m_EndPosition = new Vector2(0, m_MoveDistance);
            
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
            }
        }

        private void StopCurrentAnimation()
        {
            if (m_AnimationSequence != null && m_AnimationSequence.IsActive())
            {
                m_AnimationSequence.Kill();
                Debug.Log("PopupTextAnimation: Stopped current animation");
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
            m_PopupTextView = null;
            m_RectTransform = null;
            m_CanvasGroup = null;
        }
    }
}