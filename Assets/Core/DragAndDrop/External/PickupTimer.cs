using UnityEngine;
using Utils.SimpleTimerSystem.External;
using Utils.SimpleTimerSystem.Runtime;

namespace Core.DragAndDrop.External
{
    public class PickupTimer
    {
        private readonly SimpleTimerWrapper m_Timer;
        private readonly float m_DelayTime = 0.3f;
        private bool m_IsWaiting;

        public event System.Action OnTimerComplete;

        public PickupTimer(ITimerController timerController)
        {
            m_Timer = new SimpleTimerWrapper(
                timerController,
                m_DelayTime,
                OnTimerTick,
                OnTimerCompleted
            );
        }

        public void StartTimer()
        {
            m_Timer.StopTimer();
            m_Timer.ResetTimer();
            m_Timer.StartTimer();
            m_IsWaiting = true;
            Debug.Log("PickupTimer: Timer started");
        }

        public void StopTimer()
        {
            m_Timer.StopTimer();
            m_IsWaiting = false;
            Debug.Log("PickupTimer: Timer stopped");
        }

        public bool IsWaiting => m_IsWaiting;

        private void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            // Do nothing
        }

        private void OnTimerCompleted(SimpleTimerInfo timerInfo)
        {
            if (m_IsWaiting)
            {
                OnTimerComplete?.Invoke();
                m_IsWaiting = false;
                Debug.Log("PickupTimer: Timer completed");
            }

            m_Timer.StopTimer();
        }
    }
}