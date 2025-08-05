using Core.BottomBlocks.External;
using Core.Tower.External;
using UnityEngine;

namespace Core.Tower.Runtime
{
    public interface ITowerPositionCalculator
    {
        bool TryCalculatePlacementPosition(BlockView blockView, Vector3 desiredPosition, TowerState towerState,
            out Vector3 finalPosition);

        Vector3 CalculateNewPositionForBlock(BlockView blockView, float baseY);
        float GetBaseYForIndex(int index, TowerState towerState);
    }
}