using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.Tower.Runtime
{
    public interface ITowerController
    {
        bool IsBlockInTowerArea(BlockView blockView, Vector3 position);
        bool TryPlaceBlockInTower(BlockView blockView, Vector3 position);
        void RemoveBlockFromTower(BlockView blockView);
        void SaveTowerState();
    }
}