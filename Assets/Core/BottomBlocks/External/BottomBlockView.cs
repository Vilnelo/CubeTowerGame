using UnityEngine;
using UnityEngine.UI;

namespace Core.BottomBlocks.External
{
    public class BottomBlockView : MonoBehaviour
    {
        [SerializeField] private Image m_BlockSprite;

        public void SetImage(Sprite sprite)
        {
            m_BlockSprite.sprite = sprite;
        }
    }
}