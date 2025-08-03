using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Tower.Runtime.Data
{
    [Serializable]
    public class TowerBlockData
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public int LayerIndex;

        public TowerBlockData(int blockId, Vector3 position, int layerIndex)
        {
            this.BlockId = blockId;
            this.PositionX = position.x;
            this.PositionY = position.y;
            this.PositionZ = position.z;
            this.LayerIndex = layerIndex;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(PositionX, PositionY, PositionZ);
        }
    }
}