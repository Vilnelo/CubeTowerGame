using System;
using Utils.SimpleTimerSystem.Runtime;

namespace Utils.SimpleTimerSystem.External
{
    public class SimpleTimerWrapper
    {
        //TODO: Улучшить добавив возможность сохранения загрузки. Сейчас простая реализация для отслеживания задержки
        private readonly ITimerController m_TimerController;
        private readonly SimpleTimer m_Timer;
        private readonly TimerOnTickCallbackService m_TickCallbackService;

        private Action<SimpleTimerInfo> m_OnTimerComplete;
        private bool m_IsAddedToController = false;

        public SimpleTimerWrapper(ITimerController timerController, float duration,
            Action<SimpleTimerInfo> onTimerTick = null, Action<SimpleTimerInfo> onTimerComplete = null,
            float tickInterval = 0.5f)
        {
            m_TimerController = timerController;
            m_Timer = new SimpleTimer(duration);
            m_OnTimerComplete = onTimerComplete;

            if (onTimerTick != null)
            {
                m_TickCallbackService = new TimerOnTickCallbackService(onTimerTick, tickInterval);
                m_Timer.AddActionToUpdateEvent(m_TickCallbackService.OnTimerTick);
            }

            m_Timer.AddActionToCompleteEvent(OnTimerCompleteInternal);
        }

        public SimpleTimerWrapper(ITimerController timerController, SimpleTimerInfo timerInfo,
            Action<SimpleTimerInfo> onTimerTick = null, Action<SimpleTimerInfo> onTimerComplete = null,
            float tickInterval = 0.5f)
        {
            m_TimerController = timerController;
            m_Timer = new SimpleTimer(timerInfo);
            m_OnTimerComplete = onTimerComplete;

            if (onTimerTick != null)
            {
                m_TickCallbackService = new TimerOnTickCallbackService(onTimerTick, tickInterval);
                m_Timer.AddActionToUpdateEvent(m_TickCallbackService.OnTimerTick);
            }

            m_Timer.AddActionToCompleteEvent(OnTimerCompleteInternal);
        }

        private void OnTimerCompleteInternal(ISimpleTimer timer)
        {
            m_OnTimerComplete?.Invoke(m_Timer.GetTimerInfo());
            m_IsAddedToController = false;
        }

        public void StartTimer()
        {
            if (!m_IsAddedToController)
            {
                m_TimerController.AddTimer(m_Timer);
                m_IsAddedToController = true;
            }

            m_Timer.Start();
            m_TickCallbackService?.ResetTimeElapsed();
        }

        public void StopTimer()
        {
            m_Timer.Stop();
        }

        public void StopAndRemoveTimer()
        {
            m_Timer.Stop();
            if (m_IsAddedToController)
            {
                m_TimerController.RemoveTimer(m_Timer);
                m_IsAddedToController = false;
            }
        }

        public void ResetTimer()
        {
            m_Timer.Reset();
            m_TickCallbackService?.ResetTimeElapsed();
        }

        public SimpleTimerInfo GetTimerInfo() => m_Timer.GetTimerInfo();
        public bool IsRunning => m_Timer.IsRunning;
    }
}