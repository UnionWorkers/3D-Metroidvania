using InputHandler;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Controller
{

    public class PlayerController : BaseEntity
    {
        private InputActionHandler<Vector2> moveInput = new();
        private InputActionHandler<Vector2> lookInput = new();
        private InputActionHandler<bool> escInput = new();
        private InputActionHandler<bool> jumpInput = new();
        private InputActionHandler<bool> interactInput = new();

        [SerializeField] HealthComponent healthComponent;
        [SerializeField] PlayerCharacterController playerCharacterController;

        private void Awake()
        {
            if (playerCharacterController == null)
            {
                playerCharacterController = GetComponent<PlayerCharacterController>();

                if (playerCharacterController == null)
                {
                    Debug.LogError("There is no player character controller");
                }
            }
        }

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
            if (moveInput.IsPressed)
            {
                //playerCharacterController.MovePlayer(moveInput.GetReturnValue().normalized);
                playerCharacterController.SimpleMovePlayer(moveInput.GetReturnValue().normalized);
            }
            else
            {
                //playerCharacterController.MovePlayer(Vector2.zero);
                playerCharacterController.SimpleMovePlayer(Vector2.zero);
            }
        }


        private void PlayerMove(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:
                    playerCharacterController.SimpleMovePlayer(moveInput.GetReturnValue().normalized);

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
                    if (GameManager.Instance.CurrentGameState == GameState.Running)
                    {
                        GameManager.Instance.ChangeGameState(GameState.Paused);
                    }
                    else if (GameManager.Instance.CurrentGameState == GameState.Paused)
                    {
                        GameManager.Instance.ChangeGameState(GameState.Running);
                    }
                    break;
            }
        }

    }

}
