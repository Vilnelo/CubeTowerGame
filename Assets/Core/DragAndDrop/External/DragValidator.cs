using Core.BottomBlocks.External;
using Core.DragAndDrop.Runtime;
using Core.Tower.Runtime;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class DragValidator : IDragValidator
    {
        private readonly ITowerController m_TowerController;
        private readonly IAnimationChecker m_AnimationChecker;

        public DragValidator(ITowerController towerController, IAnimationChecker animationChecker)
        {
            m_TowerController = towerController;
            m_AnimationChecker = animationChecker;
        }

        public bool CanStartDrag(BlockView blockView)
        {
            var draggable = blockView.GetDraggableBlockController();
            var dragBehavior = draggable.GetDragBehavior();

            if (dragBehavior == DragType.Move && m_TowerController.IsAnyAnimationPlaying())
            {
                Debug.Log("Cannot pick up block - tower animation is playing");
                return false;
            }

            if (m_AnimationChecker.IsAnyDestructionAnimationPlaying() ||
                m_AnimationChecker.IsAnyPickupAnimationPlaying())
            {
                Debug.Log("Cannot pick up block - animation is playing");
                return false;
            }

            if (dragBehavior == DragType.Destroying)
            {
                Debug.Log("Block is being destroyed - ignoring click");
                return false;
            }

            return true;
        }
    }
}