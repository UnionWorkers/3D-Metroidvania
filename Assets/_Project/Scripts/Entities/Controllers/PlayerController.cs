using System;
using CustomCharacterController;
using Entities.CameraControl;
using InputHandler;
using Interactable;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Checkpoint;

namespace Entities.Controller
{
    public class PlayerController : BaseEntity, IHealth
    {
        // Inputs
        private InputActionHandler<Vector2> moveInput = new();
        private InputActionHandler<float> timeManipulateInput = new();
        private InputActionHandler<bool> escInput = new();
        private InputActionHandler<bool> jumpInput = new();
        private InputActionHandler<bool> interactInput = new();
        private InputActionHandler<bool> attackInput = new();
        private InputActionHandler<bool> dashInput = new();
        private InputActionHandler<bool> glideInput = new();


        // Input variables
        private bool jumpedQueued = false;
        [Range(0.01f, 0.5f)]
        [SerializeField] private float jumpInputBufferTimer;
        private float currentJumpBufferTimer;


        // Input variables
        private Vector2 moveDirection = Vector2.zero;
        private Vector2 cameraRotationDirection = Vector2.zero;

        // Interact Variables
        [SerializeField] private float interactRadius = 2f;
        [SerializeField] private LayerMask interactLayerMask;
        private const int INTERACTABLES_HIT_MAX_AMOUNT = 10;
        private IInteractable[] interactables = new IInteractable[INTERACTABLES_HIT_MAX_AMOUNT];
        private (IInteractable interactable, int index) closestInteractable = (null, -1);

        // Attack variables
        [SerializeField] private float attackCooldown = 1f;
        private float currentAttackTimer = 0;
        private bool canAttack = true;

        // Dash variables
        [SerializeField] private float dashCooldown = 1f;
        private float currentDashTimer = 0;
        private bool canDash = true;

        // Health related
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        public int GetHealth => healthComponent.CurrentHealth;

        // Components and other scripts
        [SerializeField] private HealthComponent healthComponent;

        [SerializeField] private PlayerCharacterController playerCharacterController;
        private CameraController cameraController;
        private PlayerInventory inventory = new();
        private CheckPoint currentCheckPoint;

        public Transform GetTransform => transform;
        public PlayerInventory Inventory => inventory;
        public PlayerCharacterController CharacterController => playerCharacterController;
        public HealthComponent HealthComponent => healthComponent;
        public CheckPoint CheckPoint
        {
            get => currentCheckPoint;
            set { currentCheckPoint = value; Debug.Log(currentCheckPoint); }
        }


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

            timeManipulateInput.GetAction(InputMap, "TimeManipulate");
            timeManipulateInput.OnActionPhaseChanged += TimeManipulate;

            moveInput.GetAction(InputMap, "Move");

            attackInput.GetAction(InputMap, "Attack");
            attackInput.OnActionPhaseChanged += Attack;

            dashInput.GetAction(InputMap, "Dash");
            dashInput.OnActionPhaseChanged += Dash;

            glideInput.GetAction(InputMap, "Glide");
            glideInput.OnActionPhaseChanged += Glide;

        }

        private void OnDisable()
        {
            escInput.OnDisable();
            interactInput.OnDisable();
            jumpInput.OnDisable();
            moveInput.OnDisable();
        }

        public void Heal(int inHealth)
        {
            healthComponent.CurrentHealth += inHealth;
            OnHealthChanged(healthComponent.CurrentHealth);
        }
        public void TakeDamage(DamageInfo inDamageInfo)
        {
            // Add knockback, using DamageInfo HitObject
            healthComponent.CurrentHealth -= inDamageInfo.DamageAmount;
            OnHealthChanged(healthComponent.CurrentHealth);

            if (healthComponent.CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void SetCameraController(CameraController inCameraController)
        {
            cameraController = inCameraController;
            cameraController.SetTarget(transform);
        }

        public void SwitchMoveType(MoveType inMoveType)
        {
            playerCharacterController.MoveType = inMoveType;
        }

        public override void OnInitialize()
        {
            Cursor.lockState = CursorLockMode.Locked;
            healthComponent.Initialize(); GameManager.Instance.PlayerUiHandler.OnInitialize(this);
        }

        public override void OnBeforeDestroy()
        {
            OnEntityStateChanged?.Invoke(this);
        }

        public override void OnUpdate()
        {
            if (debugState)
            {
                if (Keyboard.current.pKey.wasPressedThisFrame)
                {
                    if (playerCharacterController.MoveType == MoveType.Normal)
                    {
                        playerCharacterController.MoveType = MoveType.TestNormal;
                    }
                    else if (playerCharacterController.MoveType == MoveType.TestNormal)
                    {
                        playerCharacterController.MoveType = MoveType.Normal;
                    }
                }
            }

            if (!canAttack)
            {
                if (currentAttackTimer < attackCooldown)
                {
                    currentAttackTimer += Time.deltaTime;
                }
                else
                {
                    canAttack = true;
                }
            }

            if (CharacterController.CurrentDahsStage == DahsStage.Reset)
            {
                if (currentDashTimer < dashCooldown)
                {
                    currentDashTimer += Time.deltaTime;
                }
                else
                {
                    CharacterController.CurrentDahsStage = DahsStage.CanDash;
                }
            }

            if (jumpedQueued && currentJumpBufferTimer < jumpInputBufferTimer)
            {
                currentJumpBufferTimer += Time.deltaTime;
                JumpRequest();
            }
            else
            {
                jumpedQueued = false;
                currentJumpBufferTimer = 0f;
            }

            if (moveInput.IsPressed)
            {
                moveDirection = moveInput.GetReturnValue();
            }
            else
            {
                moveDirection = Vector2.zero;
            }

            CheckForInteractables();
        }

        public override void OnFixedUpdate()
        {
            if (playerCharacterController.CanMove)
            {
                playerCharacterController.MovePlayer(moveDirection, cameraController.transform);
            }
        }

        private void CheckForInteractables()
        {
            RaycastHit[] newInteractablesHit = new RaycastHit[INTERACTABLES_HIT_MAX_AMOUNT];
            int ObjectHit = Physics.SphereCastNonAlloc(transform.position, interactRadius, transform.forward, newInteractablesHit, default, interactLayerMask);

            // May need to check if interactables is maxed out
            for (int i = 0; i < interactables.Length; i++)
            {
                bool isInCollider = false;
                bool canAdd = false;

                IInteractable oldInteractable = interactables[i];
                (IInteractable interactable, int index) newInteractable = (null, -1);

                for (int j = 0; j < ObjectHit; j++)
                {
                    if (newInteractablesHit[j].transform == null)
                    {
                        continue;
                    }

                    newInteractable = (newInteractablesHit[j].transform.GetComponent<IInteractable>(), j);

                    if (newInteractable.interactable == null)
                    {
                        continue;
                    }

                    // contains
                    if (newInteractable.interactable == oldInteractable || newInteractable.interactable == interactables[j])
                    {
                        isInCollider = true;
                        canAdd = false;
                        newInteractablesHit[j] = default;
                        break;
                    }

                    // add
                    if (oldInteractable == null && newInteractable.interactable != interactables[j] && newInteractable.interactable.MyItemState != ItemState.Destroyed)
                    {
                        canAdd = true;
                        isInCollider = true;
                    }

                }

                // remove if not in collider range
                if (!isInCollider && oldInteractable != null && oldInteractable.MyItemState != ItemState.Destroyed)
                {
                    oldInteractable.DeHighlight();
                    interactables[i] = null;
                }
                else if (canAdd && newInteractable.interactable.MyItemState != ItemState.Destroyed)
                {
                    newInteractable.interactable.Highlight();
                    interactables[i] = newInteractable.interactable;
                    newInteractablesHit[newInteractable.index] = default;
                }
            }

            // have all valid object in front of the array, removing nulls in the middle of two valid objects
            int switchCount = 0;
            IInteractable[] tmpInteractable = new IInteractable[INTERACTABLES_HIT_MAX_AMOUNT];
            for (int i = 0; i < interactables.Length; i++)
            {
                if (interactables[i] == null)
                {
                    continue;
                }

                if (interactables[i].MyItemState == ItemState.None)
                {
                    interactables[i].Highlight();
                }

                tmpInteractable[switchCount] = interactables[i];
                switchCount++;
                interactables[i] = null;
            }
            interactables = tmpInteractable;

        }

        private (IInteractable, int) GetClosestInteractable()
        {
            if (interactables[0] == null)
            {
                return (null, -1);
            }

            (float distFromPlayer, IInteractable myInteractable, int index) bestInteractable = (Vector3.Distance(transform.position, interactables[0].GetTransform.position), interactables[0], 0);
            for (int i = 0; i < interactables.Length; i++)
            {
                if (interactables[i] == null)
                {
                    continue;
                }
                float dist = Vector3.Distance(transform.position, interactables[i].GetTransform.position);
                if (dist < bestInteractable.distFromPlayer)
                {
                    bestInteractable = (dist, interactables[i], i);
                }
            }
            return (bestInteractable.myInteractable, bestInteractable.index);
        }

        private void Jump(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Started:
                    JumpRequest();
                    break;

                case InputActionPhase.Canceled:
                    if (playerCharacterController.PressingJump)
                    {
                        playerCharacterController.PressingJump = false;
                    }

                    break;
            }
        }

        private void JumpRequest()
        {
            jumpedQueued = true;
            if (playerCharacterController.JumpStage != JumpStage.Reset)
            {
                if (playerCharacterController.JumpStage == JumpStage.CanJump)
                {
                    playerCharacterController.JumpStage = JumpStage.CommitJump;
                    playerCharacterController.PressingJump = true;
                    jumpedQueued = false;
                }
                else if (playerCharacterController.JumpStage == JumpStage.CanDoubleJump)
                {
                    playerCharacterController.JumpStage = JumpStage.CommitDoubleJump;
                    jumpedQueued = false;
                }
            }
        }

        private void Interact(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:
                    if (playerCharacterController.MoveType != MoveType.Normal)
                    {
                        SwitchMoveType(MoveType.Normal);
                        return;
                    }

                    closestInteractable = GetClosestInteractable();
                    if (closestInteractable.interactable == null)
                    {
                        return;
                    }
                    closestInteractable.interactable.DeHighlight();

                    // Gets a delegate, then run its function
                    closestInteractable.interactable.SelectInteractable()(this);
                    interactables[closestInteractable.index] = null;
                    closestInteractable = (null, -1);
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

        private void TimeManipulate(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:
                    int inputDir = timeManipulateInput.GetReturnValue() < 0 ? -1 : 1;
                    GameManager.Instance.ChangeTimeSpeed(inputDir);

                    break;
            }
        }

        private void Attack(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:
                    if (canAttack)
                    {
                        playerCharacterController.Attack();
                        canAttack = false;
                    }

                    break;
            }
        }

        private void Dash(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:
                    if (CharacterController.CurrentDahsStage == DahsStage.CanDash)
                    {
                        CharacterController.CurrentDahsStage = DahsStage.CommitDash;
                    }

                    break;

            }
        }

        private void Glide(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Performed:

                    if (playerCharacterController.CanGlide)
                    {
                        playerCharacterController.LockGlide = !playerCharacterController.LockGlide;
                    }

                    break;

            }
        }
    }

}
