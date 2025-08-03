namespace Utils.SimpleTimerSystem.Runtime
{
    public class SimpleTimerInfo
    {
        private float m_Time;
        private float m_TimeLeft;
        private float m_Delta;

        public float Time
        {
            get => m_Time;
            set => m_Time = value;
        }

        public float TimeLeft
        {
            get => m_TimeLeft;
            set => m_TimeLeft = value;
        }

        public float Delta
        {
            get => m_Delta;
            set => m_Delta = value;
        }
    }
}