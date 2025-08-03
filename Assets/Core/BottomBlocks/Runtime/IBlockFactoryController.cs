using System.Collections.Generic;
using Core.BottomBlocks.External;
using Core.BottomBlocks.Runtime.Dto;
using Core.DragAndDrop.Runtime;
using UnityEngine;

namespace Core.BottomBlocks.Runtime
{
    public interface IBlockFactoryController
    {
        BlockView CreateBlock(CubeDto cubeConfig, Transform parent, DragType dragType);
        BlockView CreateBlockById(int blockId, Transform parent, DragType dragType);
        List<BlockView> CreateAllBlocksFromConfig(Transform parent = null);
    }
}