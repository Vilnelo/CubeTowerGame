using Core.BottomBlocks.External;

namespace Core.DragAndDrop.Runtime
{
    public interface IDragValidator
    {
        bool CanStartDrag(BlockView blockView);
    }
}