using Core.InputSystem.Runtime;
using UnityEngine;

namespace Core.InputSystem.External
{
    public class InputController
    {
        private bool m_IsBlocked;
        private bool m_IsDragging;
        private bool m_IsMouseDown;
        private float m_MinDraggingMagnitude = 0.1f;
        private Vector3 m_MouseDownPosition;

        public InputResult GetInput(out Vector3 mousePosition)
        {
            var screenPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition = screenPosition;

            if (m_IsBlocked)
            {
                return InputResult.None;
            }

            if (Input.GetMouseButtonDown(0))
            {
                m_IsMouseDown = true;
                m_MouseDownPosition = mousePosition;
                return InputResult.Down;
            }

            if (Input.GetMouseButton(0))
            {
                if (m_IsMouseDown && !m_IsDragging &&
                    (mousePosition - m_MouseDownPosition).sqrMagnitude > m_MinDraggingMagnitude)
                {
                    m_IsDragging = true;
                    return InputResult.StartDrag;
                }

                if (m_IsDragging)
                {
                    return InputResult.Dragging;
                }

                return InputResult.None;
            }

            if (Input.GetMouseButtonUp(0))
            {
                m_IsMouseDown = false;
                if (m_IsDragging)
                {
                    m_IsDragging = false;
                    return InputResult.EndDrag;
                }

                return InputResult.Up;
            }

            return InputResult.None;
        }

        public bool IsMultiTouch()
        {
            return Input.touchCount > 1;
        }
    }
}