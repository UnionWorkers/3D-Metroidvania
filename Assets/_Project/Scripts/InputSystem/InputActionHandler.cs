using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace InputHandler
{
    public class InputActionHandler<T> where T : struct
    {
        InputAction inputAction;
        public Action<InputActionPhase> OnActionPhaseChanged;
        T LastReadValue;

        public void GetAction(string actionMapName, string actionName)
        {
            inputAction = InputSystem.actions.FindActionMap(actionMapName).FindAction(actionName);
            if (inputAction == null)
            {
                Debug.Log(ToString() + ": Action could not be found");
                return;
            }

            inputAction.started += PerformedAction;
            inputAction.performed += PerformedAction;
            inputAction.canceled += PerformedAction;

        }
        public InputAction GetInputAction => inputAction;
        public bool WasPressedThisFrame => inputAction.WasPressedThisFrame();
        public bool IsPressed => inputAction.IsPressed();
      
        public void OnDisable()
        {
            OnActionPhaseChanged = null;
        }

        public T GetReturnValue()
        {
            if (inputAction.expectedControlType != "")
            {
                return LastReadValue = inputAction.ReadValue<T>();
            }
            else
            {
                Debug.LogWarning(ToString() + ": This is not a value type Input action, don't use this func for this action");
                return default;
            }
        }

        private void PerformedAction(InputAction.CallbackContext context)
        {
            OnActionPhaseChanged?.Invoke(context.phase);
        }

    }
}