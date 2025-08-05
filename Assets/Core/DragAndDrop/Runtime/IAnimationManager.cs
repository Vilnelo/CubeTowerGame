using System.Collections.Generic;
using Core.Animations.External;
using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.DragAndDrop.Runtime
{
    public interface IAnimationManager
    {
        PickUpAnimation GetOrCreatePickupAnimation(BlockView blockView);
        DestructionAnimation GetOrCreateDestructionAnimation(BlockView blockView);
        void CleanupPickupAnimation(BlockView blockView);
        void CleanupDestructionAnimation(BlockView blockView);
        void ForceResetScale(BlockView blockView);
        Dictionary<RectTransform, PickUpAnimation> GetPickupAnimations();
        Dictionary<RectTransform, DestructionAnimation> GetDestructionAnimations();
        void Dispose();
    }
}