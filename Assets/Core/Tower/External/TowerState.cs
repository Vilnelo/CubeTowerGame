using System.Collections.Generic;
using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.Tower.External
{
    public class TowerState
    {
        private readonly List<BlockView> m_ActiveBlocks = new List<BlockView>();
        private bool m_IsCollapseAnimationPlaying = false;
        private bool m_IsJumpAnimationPlaying = false;
        private BlockView m_BlockBeingMoved = null;

        public IReadOnlyList<BlockView> ActiveBlocks => m_ActiveBlocks;
        public bool IsCollapseAnimationPlaying => m_IsCollapseAnimationPlaying;
        public bool IsJumpAnimationPlaying => m_IsJumpAnimationPlaying;
        public BlockView BlockBeingMoved => m_BlockBeingMoved;

        public void SetCollapseAnimationPlaying(bool isPlaying)
        {
            m_IsCollapseAnimationPlaying = isPlaying;
            Debug.Log($"TowerState: Collapse animation playing: {isPlaying}");
        }

        public void SetJumpAnimationPlaying(bool isPlaying)
        {
            m_IsJumpAnimationPlaying = isPlaying;
            Debug.Log($"TowerState: Jump animation playing: {isPlaying}");
        }

        public void SetBlockBeingMoved(BlockView block)
        {
            m_BlockBeingMoved = block;
        }

        public void AddBlock(BlockView block)
        {
            if (block != null && !m_ActiveBlocks.Contains(block))
            {
                m_ActiveBlocks.Add(block);
                Debug.Log($"TowerState: Added block to tower. Total: {m_ActiveBlocks.Count}");
            }
        }

        public bool RemoveBlock(BlockView block)
        {
            bool removed = m_ActiveBlocks.Remove(block);
            if (removed)
            {
                Debug.Log($"TowerState: Removed block from tower. Remaining: {m_ActiveBlocks.Count}");
            }

            return removed;
        }

        public int GetBlockIndex(BlockView block)
        {
            return m_ActiveBlocks.IndexOf(block);
        }

        public void ClearBlocks()
        {
            m_ActiveBlocks.Clear();
            Debug.Log("TowerState: Cleared all blocks");
        }

        public void CleanupNullBlocks()
        {
            int removedCount = m_ActiveBlocks.RemoveAll(block => block == null);
            if (removedCount > 0)
            {
                Debug.Log($"TowerState: Cleaned up {removedCount} null blocks");
            }
        }

        public BlockView GetTopBlock()
        {
            if (m_ActiveBlocks.Count == 0) return null;

            BlockView topBlock = null;
            float highestY = float.MinValue;

            foreach (var block in m_ActiveBlocks)
            {
                if (block == null) continue;

                var rect = block.GetRectTransform();
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                float topY = corners[2].y;

                if (topY > highestY)
                {
                    highestY = topY;
                    topBlock = block;
                }
            }

            return topBlock;
        }
    }
}