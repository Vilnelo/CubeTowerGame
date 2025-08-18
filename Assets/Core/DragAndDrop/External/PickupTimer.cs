using System;
using Common.TimerSystem.External;
using Common.TimerSystem.External.Interfaces;
using Common.TimerSystem.External.Models;
using UniRx;
using UnityEngine;

namespace Core.DragAndDrop.External
{
    public class PickupTimer : IDisposable
    {
        private readonly ITimerController m_TimerController;
        private readonly float m_DelayTime = 0.3f;
        private CompositeDisposable m_TimerDisposables;
        
        private SimpleTimerWrapper m_Timer;

        public event System.Action OnTimerComplete;

        public PickupTimer(ITimerController timerController)
        {
            m_TimerController = timerController;
        }

        public void StartTimer()
        {
            StopTimer();
            
            m_TimerDisposables = new CompositeDisposable();
            
            m_Timer = new SimpleTimerWrapper(m_TimerController, m_DelayTime);
            
            m_Timer.OnTick
                .Sample(TimeSpan.FromSeconds(0.5f))
                .Subscribe(OnTimerTick)
                .AddTo(m_TimerDisposables);
            
            m_Timer.OnCompleted
                .Subscribe(OnTimerCompleted)
                .AddTo(m_TimerDisposables);
            
            m_Timer.StartTimer();
            
            Debug.Log("PickupTimer: Timer started");
        }

        public void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Dispose();
                m_Timer = null;
            }
            
            if (m_TimerDisposables != null)
            {
                m_TimerDisposables.Dispose();
                m_TimerDisposables = null;
            }
            
            Debug.Log("PickupTimer: Timer stopped");
        }

        private void OnTimerTick(SimpleTimerInfo timerInfo)
        {
            Debug.LogError($"PickupTimer: Timer ticked - {timerInfo.TimeLeft:F1}s left");
        }

        private void OnTimerCompleted(ISimpleTimer timer)
        {
            Debug.Log("PickupTimer: Timer completed");
            
            OnTimerComplete?.Invoke();
            
            StopTimer();
        }

        public void Dispose()
        {
            StopTimer();
        }
    }
}