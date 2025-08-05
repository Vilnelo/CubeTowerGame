using Core.Tower.External;

namespace Core.Tower.Runtime
{
    public interface ITowerSaveManager
    {
        void SaveTowerState(TowerState towerState);
        void LoadTowerState(TowerState towerState);
    }
}