using System.Collections.Generic;
using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.Tower.Runtime
{
    public interface ITowerAnimationManager
    {
        void StartJumpAnimation(BlockView blockView, Vector3 targetPosition, System.Action onComplete);
        void StartCollapseAnimation(List<GameObject> blocks, List<Vector3> targetPositions, System.Action onComplete);
        bool IsAnyAnimationPlaying();
    }
}