using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }

    public event Action<Vector2> OnZoomPerformed;
    public event Action OnInteractPerformed;
    public event Action OnInteractAlternate;
    public event Action OnPrimaryPerformed;
    public event Action OnPausePerformed;
    public event Action<int> OnSwitchSlotPerformed;
        

    private InputSystem_Actions inputActions;

    private InputAction _switchSlotAction;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        DontDestroyOnLoad(this);
    }

    private void OnEnable() {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Interact.performed += ctx => OnInteractPerformed?.Invoke();
        inputActions.Player.InteractAlternate.performed += ctx => OnInteractAlternate?.Invoke();
        inputActions.Player.Attack.performed += ctx => OnPrimaryPerformed?.Invoke();
        inputActions.Player.Pause.performed += ctx => OnPausePerformed?.Invoke();
        inputActions.Player.SwitchSlot.performed += ctx => {
            var key = (ctx.control as KeyControl).keyCode;
            int index = (int)key - (int)Key.Digit1; 
            OnSwitchSlotPerformed?.Invoke(index);
        };
        inputActions.Player.Zoom.performed += ctx => {
            Vector2 scrollDelta = ctx.ReadValue<Vector2>();
            OnZoomPerformed?.Invoke(scrollDelta);

        };

        inputActions.Enable();
    }


    public Vector2 GetMovementInputNormalized() {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        return input.normalized;
    }

    private void OnDestroy() {
        inputActions.Player.Interact.performed -= ctx => OnInteractPerformed?.Invoke();
        inputActions.Player.InteractAlternate.performed -= ctx => OnInteractAlternate?.Invoke();
        inputActions.Player.Attack.performed -= ctx => OnPrimaryPerformed?.Invoke();
        inputActions.Player.Pause.performed -= ctx => OnPausePerformed?.Invoke();
        inputActions.Player.SwitchSlot.performed -= ctx => {
            var key = (ctx.control as KeyControl).keyCode;
            int index = (int)key - (int)Key.Digit1;  // 0 for “1”, 1 for “2”, …
            OnSwitchSlotPerformed?.Invoke(index);
        };
        inputActions.Disable();
    }

}
