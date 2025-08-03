using System;
using Core.InputSystem.Runtime;
using UnityEngine;
using Zenject;

namespace Core.InputSystem.External
{
    public class InputManager : MonoBehaviour, IInitializable
    {
        private InputController m_InputController = new InputController();
        private Vector3 m_CurrentMousePosition;

        public static event Action<Vector3> OnMouseDown;
        public static event Action<Vector3> OnMouseUp;
        public static event Action<Vector3> OnStartDrag;
        public static event Action<Vector3> OnEndDrag;
        public static event Action<Vector3> OnDragging;

        public void Initialize()
        {
            Debug.Log("InputManager: Initialized");
        }

        private void Update()
        {
            var inputResult = m_InputController.GetInput(out m_CurrentMousePosition);

            if (inputResult != InputResult.None)
            {
                ProcessInputResult(inputResult);
            }
        }

        private void ProcessInputResult(InputResult inputResult)
        {
            switch (inputResult)
            {
                case InputResult.Down:
                    OnMouseDown?.Invoke(m_CurrentMousePosition);
                    break;

                case InputResult.Up:
                    OnMouseUp?.Invoke(m_CurrentMousePosition);
                    break;

                case InputResult.StartDrag:
                    OnStartDrag?.Invoke(m_CurrentMousePosition);
                    break;

                case InputResult.EndDrag:
                    OnEndDrag?.Invoke(m_CurrentMousePosition);
                    break;

                case InputResult.Dragging:
                    OnDragging?.Invoke(m_CurrentMousePosition);
                    break;
            }
        }

        private void OnDestroy()
        {
            OnMouseDown = null;
            OnMouseUp = null;
            OnStartDrag = null;
            OnEndDrag = null;
            OnDragging = null;
        }
    }
}