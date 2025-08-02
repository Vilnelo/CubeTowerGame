using System;

namespace Utils.SimpleTimerSystem
{
    public class SimpleTimer : ISimpleTimer
    {
         private static int s_NextId = 0;
        
        private readonly int m_Id;
        private readonly SimpleTimerInfo m_TimerInfo;
        
        private Action<ISimpleTimer> m_OnComplete;
        private Action<SimpleTimerInfo> m_OnUpdate;
        private bool m_IsRunning;

        public SimpleTimer(float duration)
        {
            m_Id = s_NextId++;
            m_TimerInfo = new SimpleTimerInfo
            {
                Time = duration,
                TimeLeft = duration,
                Delta = 0f
            };
        }

        public SimpleTimer(SimpleTimerInfo timerInfo)
        {
            m_Id = s_NextId++;
            m_TimerInfo = new SimpleTimerInfo
            {
                Time = timerInfo.Time,
                TimeLeft = timerInfo.TimeLeft,
                Delta = timerInfo.Delta
            };
        }

        public int GetId() => m_Id;

        public void Update(float deltaTime)
        {
            if (!m_IsRunning)
                return;

            m_TimerInfo.Delta = deltaTime;
            m_TimerInfo.TimeLeft -= deltaTime;

            // Вызываем update коллбек
            m_OnUpdate?.Invoke(m_TimerInfo);

            if (m_TimerInfo.TimeLeft <= 0f)
            {
                m_TimerInfo.TimeLeft = 0f;
                m_IsRunning = false;
                m_OnComplete?.Invoke(this);
            }
        }

        public void Start()
        {
            m_IsRunning = true;
        }

        public void Stop()
        {
            m_IsRunning = false;
        }

        public void Reset()
        {
            m_TimerInfo.TimeLeft = m_TimerInfo.Time;
            m_IsRunning = false;
        }

        public void AddActionToCompleteEvent(Action<ISimpleTimer> onComplete)
        {
            m_OnComplete += onComplete;
        }

        public void RemoveActionToCompleteEvent(Action<ISimpleTimer> onComplete)
        {
            m_OnComplete -= onComplete;
        }

        public void AddActionToUpdateEvent(Action<SimpleTimerInfo> onUpdate)
        {
            m_OnUpdate += onUpdate;
        }

        public void RemoveActionToUpdateEvent(Action<SimpleTimerInfo> onUpdate)
        {
            m_OnUpdate -= onUpdate;
        }

        public SimpleTimerInfo GetTimerInfo() => m_TimerInfo;
        public bool IsRunning => m_IsRunning;
    }
}