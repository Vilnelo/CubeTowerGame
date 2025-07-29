using UnityEngine;

namespace Core.Canvases.Runtime
{
    public interface IMainCanvas
    {
        Transform GetTransform();
        Canvas GetCanvas();
    }
}