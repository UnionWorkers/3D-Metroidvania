using CustomCharacterController;
using Entities.CameraControl;
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
        private PlayerCharacterController playerCharacterController;
        private CameraController cameraController;


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
            lookInput.GetAction(InputMap, "Look");

        }
        private void OnDisable()
        {
            escInput.OnDisable();
        }

        public void SetCameraController(CameraController inCameraController)
        {
            cameraController = inCameraController;
            cameraController.SetTarget(transform);
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
            if(lookInput.GetReturnValue() != Vector2.zero)
            {
                cameraController.RotateCamera(lookInput.GetReturnValue());
            }

            if (moveInput.IsPressed)
            {
                playerCharacterController.MovePlayer(moveInput.GetReturnValue(), cameraController.Rotation);
                //playerCharacterController.SimpleMovePlayer(moveInput.GetReturnValue());
            }
            else
            {
                playerCharacterController.MovePlayer(Vector2.zero, cameraController.Rotation);
                //playerCharacterController.SimpleMovePlayer(Vector2.zero);
            }
        }

        private void Jump(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:
                    playerCharacterController.Jump();
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
