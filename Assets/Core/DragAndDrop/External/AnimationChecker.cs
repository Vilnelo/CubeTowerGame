using System.Collections.Generic;
using System.Linq;
using Core.Animations.External;
using Core.DragAndDrop.Runtime;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class AnimationChecker : IAnimationChecker
    {
        private readonly Dictionary<RectTransform, PickUpAnimation> m_PickupAnimations;
        private readonly Dictionary<RectTransform, DestructionAnimation> m_DestructionAnimations;

        public AnimationChecker(
            Dictionary<RectTransform, PickUpAnimation> pickupAnimations,
            Dictionary<RectTransform, DestructionAnimation> destructionAnimations)
        {
            m_PickupAnimations = pickupAnimations;
            m_DestructionAnimations = destructionAnimations;
        }

        public bool IsAnyDestructionAnimationPlaying()
        {
            foreach (var animation in m_DestructionAnimations.Values)
            {
                if (animation.IsAnimating())
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAnyPickupAnimationPlaying()
        {
            foreach (var animation in m_PickupAnimations.Values)
            {
                if (animation.IsAnimating())
                {
                    return true;
                }
            }

            return false;
        }
    }
}