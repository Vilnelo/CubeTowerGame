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
        public int TowerBlockInstanceId;

        public TowerBlockData(int blockId, Vector3 position, int layerIndex, int towerBlockInstanceId)
        {
            this.BlockId = blockId;
            this.PositionX = position.x;
            this.PositionY = position.y;
            this.PositionZ = position.z;
            this.LayerIndex = layerIndex;
            this.TowerBlockInstanceId = towerBlockInstanceId;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(PositionX, PositionY, PositionZ);
        }
    }
}