using TMPro;
using UnityEngine;

namespace Core.Animations.External.PopupText.External
{
    public class PopupTextView : MonoBehaviour
    {
        [SerializeField] private RectTransform m_RectTransform;
        [SerializeField] private TMP_Text m_Text;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        public RectTransform GetRectTransform()
        {
            return m_RectTransform;
        }

        public CanvasGroup GetCanvasGroup()
        {
            return m_CanvasGroup;
        }

        public void SetText(string text)
        {
            m_Text.text = text;
        }
    }
}