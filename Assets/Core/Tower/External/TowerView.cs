using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerView : MonoBehaviour
    {
        [SerializeField] private RectTransform m_Tower;
        
        public RectTransform Tower => m_Tower;
    }
}