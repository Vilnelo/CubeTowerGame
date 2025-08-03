using Core.InputSystem.External;
using UnityEngine;
using UnityEngine.UI;

namespace Core.BottomBlocks.External
{
    public class ScrollBlocker
    {
        private ScrollRect m_BottomScrollRect;
        private bool m_IsBlocked = false;

        public void Init(ScrollRect scrollRect)
        {
            m_BottomScrollRect = scrollRect;

            Debug.Log("ScrollBlocker: Initialized with scroll rect");
        }

        public void BlockScroll()
        {
            if (m_BottomScrollRect != null && !m_IsBlocked)
            {
                m_BottomScrollRect.enabled = false;
                m_IsBlocked = true;
                Debug.Log("ScrollBlocker: Blocked scroll");
            }
        }

        public void UnblockScroll()
        {
            if (m_BottomScrollRect != null && m_IsBlocked)
            {
                m_BottomScrollRect.enabled = true;
                m_IsBlocked = false;
                Debug.Log("ScrollBlocker: Unblocked scroll");
            }
        }

        public bool IsBlocked()
        {
            return m_IsBlocked;
        }
    }
}