using UnityEngine;
using UnityEngine.UI;

namespace Core.BottomBlocks.External
{
    public class BottomBlocksView : MonoBehaviour
    {
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private Transform m_ScrollContent;

        public Transform GetScrollContent()
        {
            return m_ScrollContent;
        }

        public ScrollRect GetScrollRect()
        {
            return m_ScrollRect;
        }
    }
}