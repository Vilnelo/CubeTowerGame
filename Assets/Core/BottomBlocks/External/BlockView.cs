using Core.BottomBlocks.Runtime.Dto;
using Core.DragAndDrop.External;
using UnityEngine;
using UnityEngine.UI;

namespace Core.BottomBlocks.External
{
    public class BlockView : MonoBehaviour
    {
        [SerializeField] private Image m_BlockSprite;
        [SerializeField] private RectTransform m_LayoutElement;
        [SerializeField] private DraggableBlockController m_DraggableBlockController;

        private string m_ColorName;
        private int m_Id;

        public void Init(CubeDto config)
        {
            m_Id = config.Id;
            m_ColorName = config.ColorName;
            m_DraggableBlockController.Init();

            Debug.Log($"BottomBlockView: Setup block {m_ColorName} #{m_Id}");
        }

        public string GetColorName()
        {
            return m_ColorName;
        }

        public RectTransform GetRectTransform()
        {
            return m_LayoutElement;
        }

        public Image GetBlockSprite()
        {
            return m_BlockSprite;
        }

        public GameObject GetBlockPrefab()
        {
            return gameObject;
        }

        public int GetId()
        {
            return m_Id;
        }

        public DraggableBlockController GetDraggableBlockController()
        {
            return m_DraggableBlockController;
        }

        public void SetImage(Sprite sprite)
        {
            m_BlockSprite.sprite = sprite;
        }

        public void SetMaterial(Material material)
        {
            m_BlockSprite.material = material;
        }

        public void SetSize(float size)
        {
            SetSize(size, size);
        }

        public void SetSize(float width, float height)
        {
            m_LayoutElement.sizeDelta = new Vector2(width, height);
        }
    }
}