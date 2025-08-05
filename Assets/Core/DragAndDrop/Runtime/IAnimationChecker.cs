namespace Core.DragAndDrop.Runtime
{
    public interface IAnimationChecker
    {
        bool IsAnyDestructionAnimationPlaying();
        bool IsAnyPickupAnimationPlaying();
    }
}