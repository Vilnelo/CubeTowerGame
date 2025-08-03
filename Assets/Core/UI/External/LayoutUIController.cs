using Core.UI.Runtime;
using UnityEngine;
using Zenject;

namespace Core.UI.External
{
    public class LayoutUIController : ILayoutUIController, IInitializable
    {
        [Inject] private ICoreUIController m_CoreUIController;

        private const int MAX_CUBES_IN_TOWER = 7;

        private float m_CubeWidth;
        private float m_CubeHeight;

        public void Initialize()
        {
            Debug.Log("LayoutUIController: Initialized");
            RecalculateLayout();
        }

        public float GetCubeSize()
        {
            return Mathf.Min(m_CubeWidth, m_CubeHeight);
        }

        public float GetCubeWidth()
        {
            return m_CubeWidth;
        }

        public float GetCubeHeight()
        {
            return m_CubeHeight;
        }

        public Vector2 GetCubeSizeVector()
        {
            return new Vector2(m_CubeWidth, m_CubeHeight);
        }

        public int GetMaxCubesInTower()
        {
            return MAX_CUBES_IN_TOWER;
        }

        public void OnScreenSizeChanged()
        {
            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            if (m_CoreUIController?.GetCoreView()?.TowerView == null)
            {
                Debug.LogWarning("LayoutUIController: TowerView not ready, skipping calculation");
                return;
            }

            CalculateCubeSizeFromTowerArea();

            Debug.Log($"LayoutUIController: Calculated - CubeWidth: {m_CubeWidth:F2}, " +
                      $"CubeHeight: {m_CubeHeight:F2}, Square Size: {GetCubeSize():F2}");
        }

        private void CalculateCubeSizeFromTowerArea()
        {
            var towerRect = m_CoreUIController.GetCoreView().TowerView.Tower;

            if (towerRect == null)
            {
                Debug.LogError("LayoutUIController: TowerView has no RectTransform!");
                return;
            }

            Rect towerArea = towerRect.rect;
            float towerWidth = towerArea.width;
            float towerHeight = towerArea.height;

            Debug.Log($"LayoutUIController: Tower area size - Width: {towerWidth:F2}, Height: {towerHeight:F2}");

            CalculateCubeDimensions(towerHeight);
        }

        private void CalculateCubeDimensions(float towerHeight)
        {
            float calculatedHeight = towerHeight / MAX_CUBES_IN_TOWER;

            m_CubeHeight = calculatedHeight;
            m_CubeWidth = calculatedHeight;

            Debug.Log($"LayoutUIController: Cube dimensions - " +
                      $"Width: {m_CubeWidth:F2}, Height: {m_CubeHeight:F2}");
        }
    }
}