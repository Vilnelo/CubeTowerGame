namespace Utils.SimpleTimerSystem.Runtime
{
    public interface ITimerController
    {
        void AddTimer(ISimpleTimer timer);
        
        void RemoveTimer(ISimpleTimer timer);
    }
}