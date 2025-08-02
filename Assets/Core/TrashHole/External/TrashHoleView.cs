using UnityEngine;
using UnityEngine.UI;

namespace Core.TrashHole.External
{
    public class TrashHoleView : MonoBehaviour
    {
        [SerializeField] private Image m_TrashHoleImage;
        [SerializeField] private RectTransform m_TrashHole;
        [SerializeField] private bool m_ShowDebugGizmos = true;
        [SerializeField] private float m_OvalSizeMultiplier = 1.01f;
        [SerializeField] private SimpleHoleMask m_SimpleMask;

        public RectTransform GetTrashHoleRect()
        {
            return m_TrashHole;
        }

        public Vector2 GetOvalSize()
        {
            if (m_TrashHoleImage == null)
            {
                Debug.LogWarning("TrashHoleView: TrashHoleImage is null!");
                return Vector2.zero;
            }

            RectTransform imageRect = m_TrashHoleImage.rectTransform;

            Vector2 imageSize = new Vector2(
                imageRect.rect.width,
                imageRect.rect.height
            );

            return new Vector2(
                imageSize.x * m_OvalSizeMultiplier,
                imageSize.y * m_OvalSizeMultiplier
            );
        }

        public Image GetTrashHoleImage()
        {
            return m_TrashHoleImage;
        }

        public SimpleHoleMask GetSimpleMask()
        {
            return m_SimpleMask;
        }
    }
}