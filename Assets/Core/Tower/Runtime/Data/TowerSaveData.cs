using System.Collections.Generic;
using Core.SaveSystem.Runtime;
using UnityEngine.Serialization;

namespace Core.Tower.Runtime.Data
{
    [System.Serializable]
    public class TowerSaveData : ISaveData
    {
        public List<TowerBlockData> TowerBlocks = new List<TowerBlockData>();
        
        public string GetSaveKey()
        {
            return "TowerData";
        }
    }
}