using System;

namespace Core.BottomBlocks.Runtime
{
    public static class ScrollEvents
    {
        public static event Action OnBlockScrollRequested;
        public static event Action OnUnblockScrollRequested;

        public static void RequestBlockScroll()
        {
            OnBlockScrollRequested?.Invoke();
        }

        public static void RequestUnblockScroll()
        {
            OnUnblockScrollRequested?.Invoke();
        }
    }
}