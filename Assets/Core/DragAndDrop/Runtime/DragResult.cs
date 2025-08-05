using Core.BottomBlocks.External;
using UnityEngine;

namespace Core.DragAndDrop.Runtime
{
    public class DragResult
    {
        public DragResultType ResultType { get; set; }
        public BlockView BlockView { get; set; }
        public GameObject DraggedObject { get; set; }
        public Vector3 Position { get; set; }
        public System.Action OnComplete { get; set; }
    }
}