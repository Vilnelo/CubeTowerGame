using UnityEngine;

namespace Core.Tower.External
{
    public class TowerAreaDetector
    {
        private RectTransform m_TowerAreaRect;

        public TowerAreaDetector(RectTransform towerAreaRect)
        {
            m_TowerAreaRect = towerAreaRect;
            Debug.Log($"TowerAreaDetector: Initialized with tower area: {towerAreaRect.name}");
        }

        public bool IsBlockCompletelyInside(RectTransform blockRect)
        {
            if (m_TowerAreaRect == null || blockRect == null)
            {
                Debug.LogError("TowerAreaDetector: TowerArea or Block rect is null");
                return false;
            }
            
            Vector3[] towerCorners = new Vector3[4];
            m_TowerAreaRect.GetWorldCorners(towerCorners);
            
            Vector3[] blockCorners = new Vector3[4];
            blockRect.GetWorldCorners(blockCorners);

            float towerLeft = towerCorners[0].x;
            float towerRight = towerCorners[2].x;
            float towerBottom = towerCorners[0].y;
            float towerTop = towerCorners[2].y;
            
            for (int i = 0; i < blockCorners.Length; i++)
            {
                Vector3 corner = blockCorners[i];
                
                bool isInsideHorizontally = corner.x >= towerLeft && corner.x <= towerRight;
                bool isInsideVertically = corner.y >= towerBottom && corner.y <= towerTop;
                
                if (!isInsideHorizontally || !isInsideVertically)
                {
                    Debug.Log($"TowerAreaDetector: Corner {i} is outside tower area");
                    return false;
                }
            }

            Debug.Log("TowerAreaDetector: Block is completely inside tower area");
            return true;
        }
    }
}