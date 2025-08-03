using UnityEngine;
using UnityEngine.UI;

namespace Core.BottomBlocks.External
{
    public class BottomBlocksView : MonoBehaviour
    {
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private Transform m_ScrollContent;
        [SerializeField] private RectTransform m_PopupTextParrent;

        public Transform GetScrollContent()
        {
            return m_ScrollContent;
        }

        public ScrollRect GetScrollRect()
        {
            return m_ScrollRect;
        }

        public RectTransform GetPopupTextParrent()
        {
            return m_PopupTextParrent;
        }
    }
}