using System.Collections.Generic;
using UnityEngine;

namespace Utils.SimpleTimerSystem.Runtime
{
    public class TimersHolder
    {
        private Dictionary<int, List<ISimpleTimer>> m_TimerMap = new Dictionary<int, List<ISimpleTimer>>();
        private List<ISimpleTimer> m_TimersToDelete = new List<ISimpleTimer>();
        private List<ISimpleTimer> m_TimersToAdd = new List<ISimpleTimer>();
        
        public void AddTimer(ISimpleTimer timer)
        {
            if (m_TimerMap == null)
            {
                Debug.LogError("[TimersHolder] TimerMap is null!");
                return;
            }
            
            m_TimersToAdd.Add(timer);
        }
        
        private void AddTimerInternal(ISimpleTimer timer)
        {
            int id = timer.GetId();
            if (m_TimerMap.ContainsKey(id))
            {
                if (m_TimerMap[id].Contains(timer))
                {
                    Debug.LogError($"Timer already exists in manager! id = {id}");
                    return;
                }
                
                m_TimerMap[id].Add(timer);
                timer.AddActionToCompleteEvent(OnComplete);
                return;
            }
            
            m_TimerMap.Add(id, new List<ISimpleTimer>() {timer});
            timer.AddActionToCompleteEvent(OnComplete);
        }

        private void OnComplete(ISimpleTimer timer)
        {
            m_TimersToDelete.Add(timer);
        }

        public void RemoveTimer(ISimpleTimer timer)
        {
            m_TimersToDelete.Add(timer);
        }

        private bool RemoveTimerInternal(ISimpleTimer timer)
        {
            if (m_TimerMap == null)
            {
                return false;
            }

            var timerId = timer.GetId();
            if (m_TimerMap.ContainsKey(timerId))
            {
                var timers = m_TimerMap[timerId];
                if (!timers.Remove(timer))
                {
                    Debug.LogError($"[TimersHolder] Can't remove timer with id {timerId}.");
                    return false;
                }
                
                if (timers.Count <= 0)
                {
                    m_TimerMap.Remove(timerId);
                }
                
                return true;
            }

            return false;
        }

        public void Update(float time)
        {
            if (m_TimerMap == null)
            {
                return;
            }
            
            foreach (var timer in m_TimersToAdd)
            {
                AddTimerInternal(timer);
            }
             
            m_TimersToAdd.Clear();
            
            if (m_TimerMap.Count <= 0)
            {
                return;
            }
            
            foreach (var timerList in m_TimerMap)
            {
                foreach (var timer in timerList.Value)
                {
                    timer.Update(time);
                }
            }
            
            foreach (var timer in m_TimersToDelete)
            {
                RemoveTimerInternal(timer);
                timer.RemoveActionToCompleteEvent(OnComplete);
            }
            
            m_TimersToDelete.Clear();
        }
    }
}