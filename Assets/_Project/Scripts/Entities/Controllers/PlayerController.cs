using Entities;
using InputHandler;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseEntity
{
    private InputActionHandler<Vector2> moveInput = new();
    private InputActionHandler<Vector2> lookInput = new();
    private InputActionHandler<bool> escInput = new();
    private InputActionHandler<bool> jumpInput = new();
    private InputActionHandler<bool> interactInput = new();


    [SerializeField] HealthComponent healthComponent;


    private void OnEnable()
    {
        string InputMap = "ProjectPlayer";
        escInput.GetAction(InputMap, "Pause");
        escInput.OnActionPhaseChanged += PauseGame;

        interactInput.GetAction(InputMap, "Interact");
        interactInput.OnActionPhaseChanged += Interact;

        jumpInput.GetAction(InputMap, "Jump");
        jumpInput.OnActionPhaseChanged += Jump;

        moveInput.GetAction(InputMap, "Move");
        moveInput.OnActionPhaseChanged += PlayerMove;
    }
    private void OnDisable()
    {
        escInput.OnDisable();
    }

    public override void OnInitialize()
    {

    }

    public override void OnBeforeDestroy()
    {

        OnEntityStateChanged?.Invoke(this);
    }

    public override void OnUpdate()
    {
        
    }


    private void PlayerMove(InputActionPhase phase)
    {
        switch (phase)
        {
            case InputActionPhase.Performed:
                moveInput.GetReturnValue();
                break;
        }
    }

    private void Jump(InputActionPhase phase)
    {
        switch (phase)
        {
            case InputActionPhase.Performed:
                Debug.LogWarning("Jump not implemented");
                break;
        }
    }

    private void Interact(InputActionPhase phase)
    {
        switch (phase)
        {
            case InputActionPhase.Performed:
                Debug.LogWarning("Interact not implemented");
                break;
        }
    }

    private void PauseGame(InputActionPhase phase)
    {
        switch (phase)
        {
            case InputActionPhase.Performed:
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    if (GameManager.Instance.CurrentGameState == GameState.Running)
                    {
                        GameManager.Instance.ChangeGameState(GameState.Paused);
                    }
                    else if (GameManager.Instance.CurrentGameState == GameState.Paused)
                    {
                        GameManager.Instance.ChangeGameState(GameState.Running);
                    }
                }
                break;
        }
    }

}
