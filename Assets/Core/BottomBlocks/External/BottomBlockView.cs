using Core.BottomBlocks.Runtime.Dto;
using UnityEngine;
using UnityEngine.UI;

namespace Core.BottomBlocks.External
{
    public class BottomBlockView : MonoBehaviour
    {
        [SerializeField] private Image m_BlockSprite;
        [SerializeField] private LayoutElement m_LayoutElement;
        
        private string m_ColorName;
        private int m_Id;
        
        public void Init(CubeDto config)
        {
            m_Id = config.Id;
            m_ColorName = config.ColorName;

            Debug.Log($"BottomBlockView: Setup block {m_ColorName} #{m_Id}");
        }

        public string GetColorName()
        {
            return m_ColorName;
        }

        public int GetId()
        {
            return m_Id;
        }

        public void SetImage(Sprite sprite)
        {
            m_BlockSprite.sprite = sprite;
        }
        
        public void SetSize(float size)
        {
            SetSize(size, size);
        }

        public void SetSize(float width, float height)
        {
            if (m_LayoutElement != null)
            {
                m_LayoutElement.preferredWidth = width;
                m_LayoutElement.preferredHeight = height;
                m_LayoutElement.minWidth = width;
                m_LayoutElement.minHeight = height;
                
                Debug.Log($"BottomBlockView: Set Layout Element size to {width:F2}x{height:F2} for block {m_ColorName}");
            }
        }
    }
}