using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Core.Animations.External
{
    public class TowerCollapseAnimation
    {
        private List<GameObject> m_AnimatedObjects = new List<GameObject>();
        private List<Sequence> m_AnimationSequences = new List<Sequence>();
        private Action m_OnCompleteCallback;

        private const float m_AnimationDuration = 0.3f;
        private const float m_StaggerDelay = 0.05f;

        public void StartCollapseAnimation(List<GameObject> objectsToAnimate, List<Vector3> targetPositions, Action onComplete = null)
        {
            if (objectsToAnimate == null || targetPositions == null)
            {
                Debug.LogError("TowerCollapseAnimation: Objects or target positions are null!");
                onComplete?.Invoke();
                return;
            }

            if (objectsToAnimate.Count != targetPositions.Count)
            {
                Debug.LogError("TowerCollapseAnimation: Objects and target positions count mismatch!");
                onComplete?.Invoke();
                return;
            }

            if (objectsToAnimate.Count == 0)
            {
                Debug.Log("TowerCollapseAnimation: No objects to animate");
                onComplete?.Invoke();
                return;
            }

            m_AnimatedObjects = new List<GameObject>(objectsToAnimate);
            m_OnCompleteCallback = onComplete;

            Debug.Log($"TowerCollapseAnimation: Starting collapse animation for {m_AnimatedObjects.Count} blocks");

            StartAnimation(targetPositions);
        }

        private void StartAnimation(List<Vector3> targetPositions)
        {
            StopAnimation();
            
            Sequence masterSequence = DOTween.Sequence();
            int completedAnimations = 0;

            for (int i = 0; i < m_AnimatedObjects.Count; i++)
            {
                var obj = m_AnimatedObjects[i];
                var targetPos = targetPositions[i];

                if (obj == null)
                {
                    completedAnimations++;
                    continue;
                }
                
                Sequence blockSequence = DOTween.Sequence();
                
                float delay = i * m_StaggerDelay;
                
                blockSequence.AppendInterval(delay);
                
                blockSequence.Append(
                    obj.transform.DOMove(targetPos, m_AnimationDuration)
                        .SetEase(Ease.OutCubic)
                );
                
                blockSequence.OnComplete(() =>
                {
                    completedAnimations++;
                    Debug.Log($"TowerCollapseAnimation: Block animation completed. Progress: {completedAnimations}/{m_AnimatedObjects.Count}");
                    
                    if (completedAnimations >= m_AnimatedObjects.Count)
                    {
                        OnAnimationComplete();
                    }
                });

                m_AnimationSequences.Add(blockSequence);

                Debug.Log($"TowerCollapseAnimation: Started animation for block {i} to position {targetPos} with delay {delay}s");
            }
            
            if (completedAnimations >= m_AnimatedObjects.Count)
            {
                OnAnimationComplete();
            }
        }

        private void OnAnimationComplete()
        {
            Debug.Log("TowerCollapseAnimation: All block animations completed");
            m_OnCompleteCallback?.Invoke();
            Cleanup();
        }

        public void StopAnimation()
        {
            foreach (var sequence in m_AnimationSequences)
            {
                if (sequence != null && sequence.IsActive())
                {
                    sequence.Kill();
                }
            }
            m_AnimationSequences.Clear();
        }

        public bool IsAnimating()
        {
            foreach (var sequence in m_AnimationSequences)
            {
                if (sequence != null && sequence.IsActive())
                {
                    return true;
                }
            }
            return false;
        }

        private void Cleanup()
        {
            m_AnimatedObjects.Clear();
            m_AnimationSequences.Clear();
            m_OnCompleteCallback = null;
        }

        public void Dispose()
        {
            StopAnimation();
            Cleanup();
        }
    }
}