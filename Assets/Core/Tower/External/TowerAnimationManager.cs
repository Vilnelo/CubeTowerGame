using System.Collections.Generic;
using Core.Animations.External;
using Core.BottomBlocks.External;
using Core.Canvases.Runtime;
using Core.Tower.Runtime;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerAnimationManager : ITowerAnimationManager
    {
        private readonly TowerJumpAnimation m_JumpAnimation;
        private readonly TowerCollapseAnimation m_CollapseAnimation;
        private readonly TowerState m_TowerState;
        private readonly IMainCanvas m_MainCanvas;

        public TowerAnimationManager(TowerState towerState, IMainCanvas mainCanvas)
        {
            m_TowerState = towerState;
            m_MainCanvas = mainCanvas;
            m_JumpAnimation = new TowerJumpAnimation();
            m_CollapseAnimation = new TowerCollapseAnimation();
        }

        public void StartJumpAnimation(BlockView blockView, Vector3 targetPosition, System.Action onComplete)
        {
            m_TowerState.SetJumpAnimationPlaying(true);

            var canvas = m_MainCanvas.GetCanvas();
            if (canvas == null)
            {
                Debug.LogWarning("TowerAnimationManager: Canvas not found, completing without animation");
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"TowerAnimationManager: Starting jump animation for {blockView.GetColorName()}");

            m_JumpAnimation.StartJumpAnimation(blockView.gameObject, targetPosition, canvas, () =>
            {
                m_TowerState.SetJumpAnimationPlaying(false);
                onComplete?.Invoke();
            });
        }

        public void StartCollapseAnimation(List<GameObject> blocks, List<Vector3> targetPositions,
            System.Action onComplete)
        {
            m_TowerState.SetCollapseAnimationPlaying(true);

            Debug.Log($"TowerAnimationManager: Starting collapse animation for {blocks.Count} blocks");

            if (blocks.Count == 0)
            {
                m_TowerState.SetCollapseAnimationPlaying(false);
                onComplete?.Invoke();
                return;
            }

            m_CollapseAnimation.StartCollapseAnimation(blocks, targetPositions, () =>
            {
                m_TowerState.SetCollapseAnimationPlaying(false);
                onComplete?.Invoke();
            });
        }

        public bool IsAnyAnimationPlaying()
        {
            return m_TowerState.IsCollapseAnimationPlaying || m_TowerState.IsJumpAnimationPlaying;
        }
    }
}