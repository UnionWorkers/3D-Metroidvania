using CustomCharacterController;
using Entities.CameraControl;
using InputHandler;
using Interactable;
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

        // Interact Variables
        [SerializeField] private float interactRadius = 2f;
        [SerializeField] private LayerMask interactLayerMask;
        private const int INTERACTABLES_HIT_MAX_AMOUNT = 10;
        private IInteractable[] interactables = new IInteractable[INTERACTABLES_HIT_MAX_AMOUNT];

        [SerializeField] HealthComponent healthComponent;
        private PlayerCharacterController playerCharacterController;
        private CameraController cameraController;
        private IInteractable closestInteractable = null;


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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }

        public override void OnBeforeDestroy()
        {

            OnEntityStateChanged?.Invoke(this);
        }

        public override void OnUpdate()
        {
            if (lookInput.GetReturnValue() != Vector2.zero)
            {
                cameraController.RotateCamera(lookInput.GetReturnValue());
            }

            if (moveInput.IsPressed)
            {
                playerCharacterController.MovePlayer(moveInput.GetReturnValue(), cameraController.transform);
            }
            else
            {
                playerCharacterController.MovePlayer(Vector2.zero, cameraController.transform);
            }

            CheckForInteractables();
        }

        private void CheckForInteractables()
        {
            RaycastHit[] newInteractablesHit = new RaycastHit[INTERACTABLES_HIT_MAX_AMOUNT];
            int ObjectHit = Physics.SphereCastNonAlloc(transform.position, interactRadius, transform.forward, newInteractablesHit, default, interactLayerMask);

            for (int i = 0; i < interactables.Length; i++)
            {
                if (interactables[i] == null)
                {
                    break;
                }

                interactables[i].DeHighlight();
                interactables[i] = null;
            }

            for (int i = 0; i < ObjectHit; i++)
            {
                if (newInteractablesHit[i].transform == null)
                {
                    break;
                }

                IInteractable interactable = newInteractablesHit[i].transform.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Highlight();
                    interactables[i] = interactable;
                }
            }
        }

        // Set the closestInteractable
        private void GetClosestInteractable()
        {
            (float distFromPlayer, IInteractable myInteractable) bestInteractable = (Vector3.Distance(transform.position, interactables[0].GetTransform.position), interactables[0]);
            for (int i = 0; i < interactables.Length; i++)
            {
                float dist = Vector3.Distance(transform.position, interactables[i].GetTransform.position);
                if (dist < bestInteractable.distFromPlayer)
                {
                    bestInteractable = (dist, interactables[i]);
                }
            }
            closestInteractable = bestInteractable.myInteractable;
        }

        private void OnTriggerEnter(Collider other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Highlight();
                closestInteractable = interactable;
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
                    if (closestInteractable == null)
                    {
                        return;
                    }

                    GetClosestInteractable();
                    // Gets a delegate, then run its function
                    closestInteractable.SelectInteractable()();

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
