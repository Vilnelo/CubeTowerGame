using System;

namespace Utils.SimpleTimerSystem
{
    public interface ISimpleTimer
    {
        int GetId();
        void Update(float deltaTime);
        void AddActionToCompleteEvent(Action<ISimpleTimer> onComplete);
        void RemoveActionToCompleteEvent(Action<ISimpleTimer> onComplete);
    }
}