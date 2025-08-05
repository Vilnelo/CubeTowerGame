using Core.BottomBlocks.External;
using Core.Tower.External;
using UnityEngine;

namespace Core.Tower.Runtime
{
    public interface ITowerPlacementValidator
    {
        bool CanPlaceBlock(BlockView blockView, Vector3 position, TowerState towerState);
        bool ValidatePlacementConditions(BlockView blockView, Vector3 position, TowerState towerState);
    }
}