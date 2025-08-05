using System.Collections.Generic;
using Core.Animations.External;
using Core.BottomBlocks.External;
using Core.DragAndDrop.Runtime;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class AnimationManager : IAnimationManager
{
    private readonly Dictionary<RectTransform, PickUpAnimation> m_PickupAnimations = 
        new Dictionary<RectTransform, PickUpAnimation>();
    private readonly Dictionary<RectTransform, DestructionAnimation> m_DestructionAnimations = 
        new Dictionary<RectTransform, DestructionAnimation>();
    
    public Dictionary<RectTransform, PickUpAnimation> GetPickupAnimations() => m_PickupAnimations;
    public Dictionary<RectTransform, DestructionAnimation> GetDestructionAnimations() => m_DestructionAnimations;

    public PickUpAnimation GetOrCreatePickupAnimation(BlockView blockView)
    {
        var rectTransform = blockView.GetRectTransform();

        if (!m_PickupAnimations.ContainsKey(rectTransform))
        {
            var animation = new PickUpAnimation();
            animation.Initialize(rectTransform);
            m_PickupAnimations[rectTransform] = animation;
            Debug.Log("AnimationManager: Created new pickup animation for block");
        }

        return m_PickupAnimations[rectTransform];
    }

    public DestructionAnimation GetOrCreateDestructionAnimation(BlockView blockView)
    {
        var rectTransform = blockView.GetRectTransform();

        if (!m_DestructionAnimations.ContainsKey(rectTransform))
        {
            var animation = new DestructionAnimation();
            animation.Initialize(rectTransform);
            m_DestructionAnimations[rectTransform] = animation;
            Debug.Log("AnimationManager: Created new destruction animation for block");
        }

        return m_DestructionAnimations[rectTransform];
    }

    public void CleanupPickupAnimation(BlockView blockView)
    {
        if (blockView == null) return;

        var rectTransform = blockView.GetRectTransform();
        if (m_PickupAnimations.ContainsKey(rectTransform))
        {
            m_PickupAnimations[rectTransform].Dispose();
            m_PickupAnimations.Remove(rectTransform);
            Debug.Log("AnimationManager: Cleaned up pickup animation for block");
        }
    }

    public void CleanupDestructionAnimation(BlockView blockView)
    {
        if (blockView == null) return;

        var rectTransform = blockView.GetRectTransform();
        if (m_DestructionAnimations.ContainsKey(rectTransform))
        {
            m_DestructionAnimations[rectTransform].Dispose();
            m_DestructionAnimations.Remove(rectTransform);
            Debug.Log("AnimationManager: Cleaned up destruction animation for block");
        }
    }

    public void ForceResetScale(BlockView blockView)
    {
        var animation = GetOrCreatePickupAnimation(blockView);
        if (!animation.IsAtReferenceScale())
        {
            animation.ForceResetToReferenceScale();
            Debug.Log("AnimationManager: Force reset scale for block");
        }
    }

    public void Dispose()
    {
        foreach (var animation in m_PickupAnimations.Values)
        {
            animation.Dispose();
        }

        foreach (var animation in m_DestructionAnimations.Values)
        {
            animation.Dispose();
        }

        m_PickupAnimations.Clear();
        m_DestructionAnimations.Clear();
    }
}
}