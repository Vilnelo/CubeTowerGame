using System;

namespace Utils.SimpleTimerSystem.Runtime
{
    public class TimerOnTickCallbackService
    {
        private readonly Action<SimpleTimerInfo> m_OnTickAction;
        private readonly float m_Interval;
        
        private float m_TimeElapsed = 0;

        public TimerOnTickCallbackService(Action<SimpleTimerInfo> onTickAction, float interval = 0.5f)
        {
            m_OnTickAction = onTickAction;
            m_Interval = interval;
        }

        public void ResetTimeElapsed()
        {
            m_TimeElapsed = 0;
        }
        
        public void ResetTime()
        {
            m_TimeElapsed = 0;
        }

        public void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            m_TimeElapsed += timerInfo.Delta;
            if (m_TimeElapsed >= m_Interval)
            {
                m_OnTickAction?.Invoke(timerInfo);
                m_TimeElapsed = 0f;
            }
        }
    }
}