using UnityEngine;
using Utils.SimpleTimerSystem.Runtime;
using Zenject;

namespace Utils.SimpleTimerSystem.External
{
    public class TimerController : MonoBehaviour, ITimerController
    {
        private readonly TimersHolder m_TimersHolder = new TimersHolder();
        
        public void AddTimer(ISimpleTimer timer)
        {
            m_TimersHolder.AddTimer(timer);
        }

        public void RemoveTimer(ISimpleTimer timer)
        {
            m_TimersHolder.RemoveTimer(timer);
        }
        
        private void Update()
        {
            m_TimersHolder.Update(Time.unscaledDeltaTime);
        }
    }
}