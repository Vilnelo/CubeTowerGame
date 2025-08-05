using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.DragAndDrop.Runtime
{
    public interface IDragResultResolver
    {
        DragResult ResolveDragResult(BlockView blockView, GameObject draggedObject, Vector3 endPosition);
    }
}