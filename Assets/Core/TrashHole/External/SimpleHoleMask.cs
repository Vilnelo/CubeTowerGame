using UnityEngine;
using UnityEngine.UI;

namespace Core.TrashHole.External
{
    public class SimpleHoleMask : MonoBehaviour
    {
        [SerializeField] private RectTransform m_HoleArea;
        
        private GameObject m_MaskContainer;
        private Image m_MaskImage;
        
        public void Init()
        {
            CreateBottomOvalMask();
        }
        
        private void CreateBottomOvalMask()
        {
            m_MaskContainer = new GameObject("HoleMask");
            m_MaskContainer.transform.SetParent(transform);
            
            RectTransform maskRect = m_MaskContainer.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            
            // Добавляем Mask компонент
            Mask mask = m_MaskContainer.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // Создаем маску-изображение
            GameObject maskImageGO = new GameObject("MaskImage");
            maskImageGO.transform.SetParent(m_MaskContainer.transform);
            
            m_MaskImage = maskImageGO.AddComponent<Image>();
            m_MaskImage.sprite = CreateInvertedBottomOvalSprite();
            
            RectTransform imageRect = maskImageGO.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
        }
        
        private Sprite CreateInvertedBottomOvalSprite()
        {
            int size = 512;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.6f); // Центр овала выше середины
            float radiusX = size * 0.35f; // Ширина овала
            float radiusY = size * 0.25f; // Высота овала
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    
                    // Расстояние от центра овала
                    float distanceX = (pos.x - center.x) / radiusX;
                    float distanceY = (pos.y - center.y) / radiusY;
                    float ovalDistance = distanceX * distanceX + distanceY * distanceY;
                    
                    // Проверяем: внутри овала И в нижней половине
                    bool isInBottomOval = ovalDistance <= 1.0f && y < center.y;
                    
                    // ИНВЕРТИРОВАННАЯ маска: 
                    // Белый (видимый) везде, кроме нижней части овала
                    Color color = isInBottomOval ? Color.clear : Color.white;
                    
                    // Добавляем мягкие края
                    if (ovalDistance > 0.8f && ovalDistance <= 1.0f && y < center.y)
                    {
                        float alpha = Mathf.Lerp(0f, 1f, (ovalDistance - 0.8f) / 0.2f);
                        color = new Color(1f, 1f, 1f, alpha);
                    }
                    
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        }
        
        public void AddObjectToMask(GameObject obj)
        {
            if (obj != null && m_MaskContainer != null)
            {
                obj.transform.SetParent(m_MaskContainer.transform);
            }
        }
        
        public void RemoveObjectFromMask(GameObject obj)
        {
            if (obj != null)
            {
                obj.transform.SetParent(null);
            }
        }
    }
}