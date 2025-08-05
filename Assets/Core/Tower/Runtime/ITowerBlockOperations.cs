using Core.BottomBlocks.External;
using Core.Tower.External;
using UnityEngine;

namespace Core.Tower.Runtime
{
    public interface ITowerBlockOperations
    {
        bool PlaceBlock(BlockView blockView, Vector3 position, TowerState towerState);
        void RemoveBlock(BlockView blockView, TowerState towerState);
    }
}