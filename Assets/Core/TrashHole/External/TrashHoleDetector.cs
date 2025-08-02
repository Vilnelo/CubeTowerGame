using UnityEngine;

namespace Core.TrashHole.External
{
    public class TrashHoleDetector
    {
        private RectTransform m_TrashHoleRect;
        private Vector2 m_OvalSize;
        
        public TrashHoleDetector(RectTransform trashHoleRect, Vector2 ovalSize)
        {
            m_TrashHoleRect = trashHoleRect;
            m_OvalSize = ovalSize;
        }
        
        public bool IsBlockTouchingHole(RectTransform blockRect)
        {
            if (m_TrashHoleRect == null || blockRect == null)
                return false;
            
            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);
            
            for (int i = 0; i < blockCorners.Length; i++)
            {
                if (IsWorldPointInOval(blockCorners[i]))
                {
                    return true;
                }
            }
            
            Vector3 blockCenter = blockRect.TransformPoint(blockRect.rect.center);
            if (IsWorldPointInOval(blockCenter))
            {
                return true;
            }
            
            Vector3 topCenter = Vector3.Lerp(blockCorners[1], blockCorners[2], 0.5f);
            Vector3 bottomCenter = Vector3.Lerp(blockCorners[0], blockCorners[3], 0.5f);
            Vector3 leftCenter = Vector3.Lerp(blockCorners[0], blockCorners[1], 0.5f);
            Vector3 rightCenter = Vector3.Lerp(blockCorners[2], blockCorners[3], 0.5f);
            
            return IsWorldPointInOval(topCenter) || 
                   IsWorldPointInOval(bottomCenter) || 
                   IsWorldPointInOval(leftCenter) || 
                   IsWorldPointInOval(rightCenter);
        }
        
        private bool IsWorldPointInOval(Vector3 worldPoint)
        {
            Vector3 ovalCenterWorld = m_TrashHoleRect.TransformPoint(m_TrashHoleRect.rect.center);
            
            Vector3 localPoint = worldPoint - ovalCenterWorld;
            
            Vector2 worldOvalSize = GetWorldOvalSize();
            
            float normalizedX = localPoint.x / (worldOvalSize.x * 0.5f);
            float normalizedY = localPoint.y / (worldOvalSize.y * 0.5f);
            
            return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1f;
        }
        
        private Vector2 GetWorldOvalSize()
        {
            Vector3 scale = m_TrashHoleRect.lossyScale;
            return new Vector2(
                m_OvalSize.x * scale.x,
                m_OvalSize.y * scale.y
            );
        }
        
        public Vector3 GetHoleCenterWorldPosition()
        {
            return m_TrashHoleRect.TransformPoint(m_TrashHoleRect.rect.center);
        }
        
        public Vector3 GetHoleBottomWorldPosition()
        {
            return m_TrashHoleRect.TransformPoint(new Vector3(0, m_TrashHoleRect.rect.yMin, 0));
        }
    }
}