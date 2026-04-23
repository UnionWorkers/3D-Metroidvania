using System;
using UnityEngine;
using UnityEngine.VFX;

namespace CustomCharacterController
{
    public enum MoveType : byte
    {
        Normal,
        OnRope,
        OnStandingPoint,
        OnClimbable,
        TestNormal
    }

    public enum JumpStage : byte
    {
        CanJump,
        CommitJump,
        CanDoubleJump,
        CommitDoubleJump,
        Reset
    }

    public enum DashStage : byte
    {
        CanDash,
        CommitDash,
        IsDashing,
        CancelDash,
        Reset
    }


    [System.Serializable]
    public struct MoveStats
    {
        // Gravity values
        [Header("Gravity values")]
        [SerializeField] private float gravityValue;
        [Range(1f, 10f)]
        [SerializeField] private float gravityMultiplier;
        [Range(0.1f, 5f)]
        [SerializeField] private float variableJumpGravityIncrease;
        [SerializeField] private float maxFallSpeed;

        // Jump values
        [Space(15)]
        [Header("Jump values")]
        [SerializeField] private float jumpHeight;

        [Range(0.1f, 2f)]
        [SerializeField] private float doubleJumpEffect;

        [Range(0.2f, 8f)]
        [SerializeField] private float timeToApex;
        [Range(0.1f, 1f)]
        [SerializeField] private float maxCoyoteTime;

        // Move values
        [Space(15)]
        [Header("Move values")]
        [SerializeField] private float maxSpeed;
        [SerializeField] private float smoothTurningForce;

        [Header("Time to reach max speed")]
        [Range(0.01f, 0.99f)]
        [SerializeField] private float accelerationTime;

        [Header("Time to reach 0 speed")]
        [Range(0.01f, 0.99f)]
        [SerializeField] private float decelerationTime;

        [Header("Height off object that can be stepped up on")]
        [SerializeField] private float stepupOffset;

        [Header("How fast the character rotates")]
        [SerializeField] private float rotationSmoothStep;

        // Air control values
        [Space(15)]
        [Header("Air control values")]

        [Range(0.01f, 2f)]
        [SerializeField] private float airAccelerateLimiter;
        [Range(0.01f, 2f)]
        [SerializeField] private float airMaxAccelerateLimiter;
        [Range(0.01f, 2f)]
        [SerializeField] private float airTurnLimiter;

        [Space(15)]
        [Header("Slope values")]
        [SerializeField] private float slideFriction;
        [SerializeField] private float slidSpeed;


        [Space(15)]
        [Header("Glide values")]
        [Range(0.001f, 1f)]
        [SerializeField] private float glideGravityLimiter;
        [SerializeField] private float glideMaxFallSpeed;

        [Space(15)]
        [Header("Dash values")]
        [SerializeField] private float dashMaxTimer;
        [SerializeField] private float dashDistance;
        [SerializeField] private LayerMask dashLayerMask;

        public float GravityValue => gravityValue;
        public float GravityMultiplier => gravityMultiplier;
        public float MaxFallSpeed => Mathf.Abs(maxFallSpeed) * -1; // Get a negative value
        public float VariableJumpGravityIncrease => variableJumpGravityIncrease;

        public float MaxSpeed => maxSpeed;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;
        public float SmoothTurningForce => smoothTurningForce;
        public float StepupOffset => stepupOffset;
        public float RotationSmoothStep => rotationSmoothStep;


        public float JumpHeight => jumpHeight;
        public float DoubleJumpEffect => doubleJumpEffect;
        public float TimeToApex => timeToApex;
        public float MaxCoyoteTime => maxCoyoteTime;

        public float AirAccelerateLimiter => airAccelerateLimiter;
        public float AirMaxAccelerateLimiter => airMaxAccelerateLimiter;
        public float AirTurnLimiter => airTurnLimiter;

        public float SlideFriction => slideFriction;
        public float SlidSpeed => slidSpeed;


        public float GlideGravityLimiter => glideGravityLimiter;
        public float GlideMaxFallSpeed => Mathf.Abs(glideMaxFallSpeed) * -1;

        public float DashMaxTimer => dashMaxTimer;
        public float DashDistance => dashDistance;
        public LayerMask DashLayerMask => dashLayerMask;


    }

    public struct MovingGroundInfo
    {
        private float velocity;
        private Vector3 moveDirection;

        public Vector3 MoveVector => velocity * moveDirection;

        public MovingGroundInfo(float inVelocity, Vector3 inMoveDirection)
        {
            velocity = inVelocity;
            moveDirection = inMoveDirection;
        }
    }

    [System.Serializable]
    public struct AttackInfo
    {
        [SerializeField] private float forwardDistance;
        [SerializeField] private float radius;
        [SerializeField] private DamageStruct damageStruct;

        public float ForwardDistance => forwardDistance;
        public float Radius => radius;
        public DamageStruct DamageStruct => damageStruct;
    }

    [System.Serializable]
    public struct ClimbingInfo
    {
        [SerializeField] private float climbSpeed;
        [SerializeField] private float climbAccelerationTime;
        [SerializeField] private float climbAroundSpeed;

        public float ClimbSpeed => climbSpeed;
        public float ClimbAccelerationTime => climbAccelerationTime;
        public float ClimbAroundSpeed => climbAroundSpeed;
    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterController : MonoBehaviour
    {

        #region SerializedField
        [SerializeField] bool debugState = false;
        [SerializeField] private CharacterController characterController;

        [Space(15)]
        [SerializeField] private bool usePreset = false;
        [SerializeField] private SOPlayerMoveData movePreset;
        [SerializeField] private MoveStats moveStats;


        [Space(15)]
        [SerializeField] private AttackInfo attackInfo;

        [Space(15)]
        [Header("Moving Ground")]
        [SerializeField] private LayerMask movingGroundLayerMask;

        // Attack
        [Space(15)]
        [Header("Attack")]
        [SerializeField] private VisualEffect attackVFX;

        // Climb
        [Space(15)]
        [Header("Climb")]
        [SerializeField] private ClimbingInfo climbingInfo;

        #endregion

        #region Private

        // Movement
        private bool canMove = true;
        private MoveType currentMoveType = MoveType.Normal;
        private float currentVelocity;
        private Vector3 currentMoveDirection;
        private Vector3 wantedMoveDirection;
        private float wantedRotation;
        private float currentRotation;
        private float rotationVelocity;

        // Forces
        private Vector3 externalForces;
        private Vector3 slopeForce;
        private Vector3 finalForce;

        // Gravity 
        private float gravityScale = 1f;

        // Jump
        private JumpStage jumpStage = JumpStage.CanJump;
        private float currentCoyoteTime = 0;

        // Moving ground 
        private MovingGroundInfo movingGroundInfo = new(0, Vector3.zero);

        // Slope 
        private bool isOnSlope;
        private Vector3 groundHitNormal;

        // Dash
        private float currentDashTimer = 0f;
        private Vector3 dashDirection = Vector3.zero;
        private float dahsRotationDirection = 0;


        private Vector3 t_lastPos;
        private Transform T_Ground;
        #endregion

        #region Public

        [NonSerialized] public RopeInteractable RopeObject = null;
        [NonSerialized] public ClimbableInteractable ClimbableObject = null;
        [NonSerialized] public StandingPointInteractable StandingPointObject = null;
        [NonSerialized] public bool CanGlide = false;
        [NonSerialized] public bool LockGlide = false;
        [NonSerialized] public bool PressingJump = false;
        [NonSerialized] public DashStage CurrentDashStage;
        [Space(15)]
        public PlayerAnimationController AnimationController = new();

        public bool CanMove => canMove;
        public bool IsGround => characterController.isGrounded;
        public float BottomPos => characterController.height * 0.5f - characterController.center.y;
        public float MiddlePos => characterController.height * 0.5f;

        public MoveStats MoveStats
        {
            get => moveStats;
            set => moveStats = value;
        }
        public SOPlayerMoveData MovePreset
        {
            get => movePreset;
            set => movePreset = value;
        }
        public MoveType MoveType
        {
            get => currentMoveType;
            set
            {
                if (value == currentMoveType) { return; }

                // Cleaning current MoveType 
                switch (currentMoveType)
                {
                    case MoveType.Normal:
                        canMove = false;
                        break;
                    case MoveType.OnRope:
                        RopeObject = null;
                        break;
                    case MoveType.OnStandingPoint:
                        StandingPointObject = null;
                        break;
                    case MoveType.OnClimbable:
                        ClimbableObject = null;
                        break;
                }

                finalForce = Vector3.zero;
                currentVelocity = 0;
                currentMoveDirection = Vector3.zero;
                wantedMoveDirection = Vector3.zero;
                externalForces = Vector3.zero;
                slopeForce = Vector3.zero;
                finalForce = Vector3.zero;

                // Initialize next MoveType 
                switch (value)
                {
                    case MoveType.Normal:
                        canMove = true;
                        break;
                    case MoveType.OnRope:
                        AttachToRope();
                        break;
                    case MoveType.OnStandingPoint:
                        AttachStandingPoint();
                        break;
                    case MoveType.OnClimbable:
                        AttachClimbable();
                        break;
                }

                currentMoveType = value;
            }
        }

        public JumpStage JumpStage
        {
            get => jumpStage;
            set
            {
                if (jumpStage == value)
                {
                    return;
                }

                jumpStage = value;
            }
        }


        #endregion

        #region Debug
        private void OnDrawGizmos()
        {
            if (debugState)
            {
                if (currentMoveType == MoveType.Normal)
                {
                    Vector3 wantedMoveDir = (wantedMoveDirection * moveStats.MaxSpeed) + transform.position;
                    Vector3 moveDirection = (currentMoveDirection * currentVelocity) + transform.position;
                    Vector3 externalDir = externalForces + transform.position;
                    Vector3 finalDir = (finalForce * 2) + transform.position;

                    moveDirection.y = transform.position.y;

                    Gizmos.color = new Color(0.5f, 0f, 0f, 0.7f);
                    Gizmos.DrawSphere(wantedMoveDir, 0.4f);
                    Gizmos.DrawLine(transform.position, wantedMoveDir);

                    Gizmos.color = new Color(0f, 0.5f, 0f, 0.7f);
                    Gizmos.DrawSphere(moveDirection, 0.4f);
                    Gizmos.DrawLine(transform.position, moveDirection);

                    Gizmos.color = new Color(0f, 0, 0.5f, 0.7f);
                    Gizmos.DrawSphere(externalDir, 0.4f);
                    Gizmos.DrawLine(transform.position, externalDir);

                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    Gizmos.DrawSphere(finalDir, 0.6f);
                    Gizmos.DrawLine(transform.position, finalDir);
                }

                //Physics.CapsuleCast(transform.position, transform.position + (transform.forward * 0.1f), attackInfo.Radius, transform.forward, attackInfo.ForwardDistance);
            }

        }
        #endregion

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();

                if (characterController == null)
                {
                    Debug.LogError("There is no character controller on Player");
                }
            }

            if (AnimationController.CharacterAnimator == null)
            {
                AnimationController.CharacterAnimator = GetComponentInChildren<Animator>();

                if (AnimationController.CharacterAnimator == null)
                {
                    Debug.LogError("There is no Animator on Player");
                }
            }

            // turn off CharacterController, using own implementation
            characterController.stepOffset = 0;

            if (usePreset && movePreset)
            {
                moveStats = movePreset.MoveStats;
            }

        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // for slope detection 
            groundHitNormal = hit.normal;

            // make the character fall 
            if (transform.position.y - hit.transform.position.y < 0)
            {
                if (externalForces.y > 0)
                {
                    externalForces.y = 0;
                }
            }

        }

        public void MovePlayer(Vector2 inDirection, Transform cameraTransform)
        {
            switch (currentMoveType)
            {
                case MoveType.Normal:
                    NormalMovement(ref inDirection, ref cameraTransform);
                    break;
                case MoveType.OnRope:
                    // Fix 💀
                    RopeMovement(ref inDirection, ref cameraTransform);
                    break;

                case MoveType.OnStandingPoint:
                    StandingPointMovement(ref inDirection, ref cameraTransform);
                    break;

                case MoveType.OnClimbable:
                    ClimbingMovement(ref inDirection, ref cameraTransform);
                    break;

                case MoveType.TestNormal:
                    MoveType = MoveType.Normal;
                    //                    TestNormalMovement(ref inDirection, ref cameraTransform);
                    break;
            }
        }

        private void NormalMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            if (!canMove)
            {
                return;
            }

            // Input handling
            if (inDirection != Vector2.zero)
            {
                wantedRotation = Mathf.Atan2(inDirection.x, inDirection.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                wantedMoveDirection = Quaternion.Euler(0, wantedRotation, 0) * Vector3.forward;
                wantedMoveDirection = wantedMoveDirection.normalized;
            }
            else
            {
                wantedMoveDirection.x = 0;
                wantedMoveDirection.z = 0;
            }

            // jump
            if (jumpStage == JumpStage.CommitJump || jumpStage == JumpStage.CommitDoubleJump)
            {
                CommitJump();
            }

            // Slope
            if (!isOnSlope)
            {
                // reset hit slope velocity when grounded
                groundHitNormal = Vector3.zero;
                slopeForce = Vector3.zero;
            }
            else
            {
                // Check for slope
                if (groundHitNormal != Vector3.zero)
                {
                    CalculateSlopeVector();
                    groundHitNormal = Vector3.zero;
                    CanGlide = false;
                    LockGlide = false;
                }
            }

            // When is grounded
            if (characterController.isGrounded && !isOnSlope)
            {
                WhenPlayerGrounded();
                CanGlide = false;
                LockGlide = false;
            }
            // Coyote
            else if (currentCoyoteTime <= moveStats.MaxCoyoteTime)
            {
                movingGroundInfo = new(0, Vector3.zero);
                T_Ground = null;
                currentCoyoteTime += Time.fixedDeltaTime;
                CanGlide = true;
            }
            // can double jump
            else if (jumpStage != JumpStage.Reset && jumpStage != JumpStage.CanDoubleJump && CurrentDashStage != DashStage.IsDashing)
            {
                JumpStage = JumpStage.CanDoubleJump;
                CanGlide = true;
            }

            // Gravity 
            ApplyGravity();

            // Apply forces
            finalForce = (MoveToWantedPoint() + externalForces + movingGroundInfo.MoveVector) * Time.fixedDeltaTime;


            if (movingGroundInfo.MoveVector.y > 0 && T_Ground != null)
            {
                Vector3 platformDelta = T_Ground.position - t_lastPos;
                characterController.Move(platformDelta * 1.1f);
                t_lastPos = T_Ground.position;
            }

            // Stepup check when moving 
            bool canStepUp = false;
            if (characterController.isGrounded && inDirection != Vector2.zero)
            {
                canStepUp = CanStepUp();
            }

            // add slope force
            if (!canStepUp && isOnSlope)
            {
                finalForce += slopeForce * Time.fixedDeltaTime;
            }

            CommitDash();

            if (finalForce.y < 0 && !characterController.isGrounded)
            {
                AnimationController.IsFalling(finalForce.y);
            }
            else
            {
                AnimationController.IsFalling(1);
            }


            characterController.Move(finalForce);

            // is the angel of the ground lower then the slop limit
            isOnSlope = Vector3.Angle(Vector3.up, groundHitNormal) >= characterController.slopeLimit;
        }

        private void ApplyGravity()
        {
            float downVelocity;

            gravityScale = (2 * moveStats.JumpHeight) / (moveStats.TimeToApex * moveStats.TimeToApex);

            // gravity when Y is negative, character falling 
            if (!characterController.isGrounded && externalForces.y < 0f)
            {
                // Gliding 
                if (LockGlide && CanGlide)
                {
                    gravityScale = (gravityScale * moveStats.GravityMultiplier) * moveStats.GlideGravityLimiter;
                    downVelocity = moveStats.GravityValue;
                }
                else
                {
                    gravityScale = gravityScale * moveStats.GravityMultiplier;
                    downVelocity = moveStats.GravityValue;
                }
            }
            // force character down faster, variable jump  
            else if (!PressingJump && externalForces.y > 0.05f && jumpStage == JumpStage.CanDoubleJump)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier * moveStats.VariableJumpGravityIncrease;
                downVelocity = moveStats.GravityValue;
            }
            // gravity when Y is positive, character going up 
            else
            {
                downVelocity = moveStats.GravityValue;
            }

            if (LockGlide && CanGlide)
            {
                if (externalForces.y >= moveStats.GlideMaxFallSpeed)
                {
                    externalForces.y += downVelocity * gravityScale * Time.fixedDeltaTime;
                }
                else
                {
                    externalForces.y = moveStats.GlideMaxFallSpeed;
                }
            }
            else
            {
                if (externalForces.y >= moveStats.MaxFallSpeed)
                {
                    externalForces.y += downVelocity * gravityScale * Time.fixedDeltaTime;
                }
                else
                {
                    externalForces.y = moveStats.MaxFallSpeed;
                }
            }

        }

        private void WhenPlayerGrounded()
        {
            gravityScale = 1f;
            currentCoyoteTime = 0;

            AnimationController.AnimationState = AnimationState.Grounded;

            if (wantedMoveDirection.y < 0.1f && jumpStage != JumpStage.CommitJump)
            {
                JumpStage = JumpStage.CanJump;
            }

            // Check if there is moving ground under the player
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, movingGroundLayerMask))
            {
                switch (LayerMask.LayerToName(hit.transform.gameObject.layer))
                {
                    case "MovingObject":

                        MovingObject movingObject = hit.transform.GetComponent<MovingObject>();
                        if (movingObject != null)
                        {
                            // add offset up so it docent giddier as much  
                            movingGroundInfo = new(movingObject.CurrentVelocity, movingObject.CurrentMoveDirection);
                            if (T_Ground != hit.transform)
                            {
                                T_Ground = hit.transform;
                                t_lastPos = T_Ground.position;
                            }
                            //lastPos = hit.transform.position;
                        }

                        break;
                    case "ConveyorBelt":

                        ConveyorBelt conveyorBelt = hit.transform.GetComponent<ConveyorBelt>();
                        if (conveyorBelt != null)
                        {
                            // add offset up so it docent giddier as much  
                            movingGroundInfo = new(conveyorBelt.CurrentVelocity, conveyorBelt.CurrentMoveDirection);
                        }
                        break;
                }
            }
            else if (movingGroundInfo.MoveVector != Vector3.zero)
            {
                movingGroundInfo = new(0, Vector3.zero);
                T_Ground = null;
            }

            if (externalForces.y < -2f)
            {
                externalForces.y = -2f;
            }
        }

        private Vector3 MoveToWantedPoint()
        {
            // Moving on ground
            if (characterController.isGrounded)
            {
                currentMoveDirection = Vector3.Lerp(currentMoveDirection, wantedMoveDirection, Time.fixedDeltaTime * moveStats.SmoothTurningForce);

                currentRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, wantedRotation, ref rotationVelocity, moveStats.RotationSmoothStep);
                transform.rotation = Quaternion.Euler(0, currentRotation, 0);

                if (wantedMoveDirection != Vector3.zero)
                {
                    if (currentVelocity < moveStats.MaxSpeed)
                    {
                        currentVelocity += moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.AccelerationTime);
                    }
                    else
                    {
                        currentVelocity = moveStats.MaxSpeed;
                    }
                }
                else
                {
                    if (currentVelocity > 0)
                    {
                        currentVelocity -= moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.DecelerationTime);
                    }
                    else
                    {
                        currentVelocity = 0;
                    }
                }
            }
            // Moving in the air 
            else
            {
                currentMoveDirection = Vector3.Lerp(currentMoveDirection, wantedMoveDirection, Time.fixedDeltaTime * moveStats.SmoothTurningForce * moveStats.AirTurnLimiter);

                if (wantedMoveDirection != Vector3.zero)
                {
                    if (currentVelocity < (moveStats.MaxSpeed * moveStats.AirMaxAccelerateLimiter))
                    {
                        currentVelocity += moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.AccelerationTime) * moveStats.AirAccelerateLimiter;
                    }
                    else if (currentVelocity > (moveStats.MaxSpeed * moveStats.AirMaxAccelerateLimiter))
                    {
                        currentVelocity -= moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.DecelerationTime);
                    }
                }
                else
                {
                    if (currentVelocity > 0)
                    {
                        currentVelocity -= moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.DecelerationTime);
                    }
                    else
                    {
                        currentVelocity = 0;
                    }
                }
            }

            AnimationController.SetRunAnimation(currentVelocity, this);


            return currentVelocity * currentMoveDirection;
        }


        private bool CanStepUp()
        {
            Vector3 normalizedMoveDirXZ = new Vector3(finalForce.x, 0f, finalForce.z).normalized;
            float distance = characterController.radius + characterController.skinWidth;

            Vector3 bottom = transform.position - new Vector3(0f, BottomPos, 0f);
            Vector3 stepOffsetLimit = new(bottom.x, bottom.y + moveStats.StepupOffset, bottom.z);
            //Raycast at player's ground level in direction of movement
            bool hitWithBottomRaycast = Physics.Raycast(bottom, normalizedMoveDirXZ, out RaycastHit hitBottom, distance);

            //Raycast at player's ground level + StepOffset in direction of movement
            bool hitWithTopRaycast = Physics.Raycast(stepOffsetLimit, normalizedMoveDirXZ, out RaycastHit hitTop, distance);

            if (hitWithBottomRaycast && hitWithTopRaycast)
            {
                // Cant step up on object
                return false;
            }
            else if (hitWithBottomRaycast && !hitWithTopRaycast)
            {
                // Step up on object 
                Vector3 rayStartPoint = hitBottom.point + (normalizedMoveDirXZ * 0.2f);
                rayStartPoint.y = stepOffsetLimit.y * 2f;
                bool hitOnTopOfObject = Physics.Raycast(rayStartPoint, Vector3.down, out RaycastHit objectTopHit, (rayStartPoint - hitBottom.point).magnitude);

                if (hitOnTopOfObject && objectTopHit.transform == hitBottom.transform)
                {
                    float angel = Vector3.Angle(Vector3.up, objectTopHit.normal);
                    if (angel <= 0)
                    {
                        rayStartPoint = objectTopHit.point;
                        rayStartPoint.y += characterController.height;

                        characterController.enabled = false;
                        transform.position = rayStartPoint;
                        characterController.enabled = true;
                        return true;
                    }

                }
                return false;
            }
            else
            {
                // no object to step to
                return false;
            }
        }

        // when the player is on a slope
        private void CalculateSlopeVector()
        {
            slopeForce.x += (1f - groundHitNormal.y) * groundHitNormal.x * (moveStats.SlidSpeed - moveStats.SlideFriction);
            slopeForce.z += (1f - groundHitNormal.y) * groundHitNormal.z * (moveStats.SlidSpeed - moveStats.SlideFriction);
        }

        public void CommitJump()
        {
            if (jumpStage == JumpStage.CommitJump || currentMoveType == MoveType.OnRope)
            {
                if (currentMoveType != MoveType.Normal)
                {
                    MoveType = MoveType.Normal;
                }

                LockGlide = false;
                gravityScale = (2 * moveStats.JumpHeight) / (moveStats.TimeToApex * moveStats.TimeToApex);

                externalForces.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight);

                if (CurrentDashStage == DashStage.IsDashing)
                {
                    CurrentDashStage = DashStage.CancelDash;
                }

                AnimationController.TriggerJumpAnimation(true);

                JumpStage = JumpStage.CanDoubleJump;
            }
            else if (jumpStage == JumpStage.CommitDoubleJump)
            {

                LockGlide = false;
                gravityScale = (2 * moveStats.JumpHeight) / (moveStats.TimeToApex * moveStats.TimeToApex);

                transform.rotation = Quaternion.Euler(0, wantedRotation, 0);
                currentMoveDirection = wantedMoveDirection;

                externalForces.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight) * moveStats.DoubleJumpEffect;
                JumpStage = JumpStage.Reset;

                if (CurrentDashStage == DashStage.IsDashing)
                {
                    CurrentDashStage = DashStage.CancelDash;
                }

                AnimationController.TriggerJumpAnimation(false);
            }

        }

        public void CommitDash()
        {
            if (CurrentDashStage == DashStage.CommitDash)
            {
                if (wantedMoveDirection == Vector3.zero)
                {
                    CurrentDashStage = DashStage.CanDash;
                    return;
                }

                CurrentDashStage = DashStage.IsDashing;
                dashDirection = wantedMoveDirection.normalized;
                dahsRotationDirection = wantedRotation;
                transform.rotation = Quaternion.Euler(0, dahsRotationDirection, 0);
            }

            if (CurrentDashStage == DashStage.IsDashing)
            {
                if (currentDashTimer >= moveStats.DashMaxTimer)
                {
                    CurrentDashStage = DashStage.CancelDash;
                    currentDashTimer = 0;
                }
                else
                {
                    finalForce = Vector3.zero;
                    externalForces = Vector3.zero;

                    // calculate start and end point of capsule
                    Vector3 bottom = transform.position;
                    Vector3 top = transform.position;
                    // Limit the height of the capsule for better detection 
                    float height = BottomPos * 0.4f;
                    bottom.y -= height;
                    top.y += height;

                    if (Physics.CapsuleCast(bottom, top, characterController.radius, dashDirection, 0.7f, moveStats.DashLayerMask))
                    {
                        CurrentDashStage = DashStage.CancelDash;
                        return;
                    }

                    finalForce = ((moveStats.DashDistance / moveStats.DashMaxTimer) * dashDirection) * Time.fixedDeltaTime;
                    currentDashTimer += Time.fixedDeltaTime;
                }
            }

            if (CurrentDashStage == DashStage.CancelDash)
            {
                currentDashTimer = 0;
                CurrentDashStage = DashStage.Reset;
                currentMoveDirection = dashDirection;
            }

        }

        public void Attack()
        {
            // Use the physics debug to see the hole CapsuleCast
            VisualEffect visualEffect = null;
            RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, transform.position + (transform.forward * 0.1f), attackInfo.Radius, transform.forward, attackInfo.ForwardDistance);

            foreach (RaycastHit hitInfo in hits)
            {
                if (hitInfo.transform == transform) { continue; }
                IHealth health = hitInfo.transform.GetComponent<IHealth>();

                if (health != null)
                {
                    if (visualEffect == null)
                    {
                        visualEffect = Instantiate(attackVFX, hitInfo.point, Quaternion.identity);
                        visualEffect.Play();
                    }
                    health.TakeDamage(new(attackInfo.DamageStruct.DamageAmount, transform));
                }
            }

            AnimationController.TriggerAttackAnimation();

            if (visualEffect != null)
            {
                Destroy(visualEffect.gameObject, 1f);
            }
        }

        private void AttachToRope()
        {
            canMove = false;
            characterController.enabled = false;

            transform.position = RopeObject.GetClosestPointOnSegment(transform.position);

            canMove = true;
            characterController.enabled = true;
        }

        private void RopeMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            if (!canMove)
            {
                return;
            }
            else if (RopeObject == null)
            {
                MoveType = MoveType.Normal;
                return;
            }
            else if (JumpStage == JumpStage.CommitJump)
            {
                CommitJump();
                MoveType = MoveType.Normal;
                return;
            }

            if (JumpStage != JumpStage.CanJump && JumpStage != JumpStage.CommitJump)
            {
                JumpStage = JumpStage.CanJump;
            }

            if (inDirection != Vector2.zero)
            {
                // wantedRotation = Mathf.Atan2(inDirection.x, inDirection.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                // wantedMoveDirection = Quaternion.Euler(0, wantedRotation, 0) * Vector3.forward;
                // wantedMoveDirection = wantedMoveDirection.normalized;

                float dotProduct = Vector3.Dot(cameraTransform.forward, RopeObject.MoveForward);
                if (dotProduct > 0)
                {
                    wantedMoveDirection = RopeObject.MoveForward * inDirection.y;
                }
                else
                {
                    wantedMoveDirection = -RopeObject.MoveForward * inDirection.y;
                }
            }
            else
            {
                wantedMoveDirection = Vector3.zero;
                currentMoveDirection = Vector3.zero;
            }

            float ropeDist = RopeObject.RopeDistance;
            float startToPlayer = RopeObject.DistanceFromStartToPoint(new Vector3(transform.position.x, RopeObject.StartPoint.y, transform.position.z));
            float lerpAlfa = startToPlayer / ropeDist;

            if (lerpAlfa <= 0.05)
            {
                Vector3 pointToPos = RopeObject.StartPoint - transform.position;
                float dotProduct = Vector3.Dot(transform.position.normalized, pointToPos.normalized);
                
                Debug.Log(dotProduct);

                if (dotProduct > 0)
                {
                    if (wantedMoveDirection == RopeObject.MoveForward)
                    {
                        wantedMoveDirection = Vector3.zero;
                        currentMoveDirection = Vector3.zero;
                    }
                }
                else
                {
                    if (wantedMoveDirection == -RopeObject.MoveForward)
                    {
                        wantedMoveDirection = Vector3.zero;
                        currentMoveDirection = Vector3.zero;
                    }
                }
            }
            else if (lerpAlfa >= 0.95)
            {
                Vector3 pointToPos = RopeObject.EndPoint - transform.position;
                float dotProduct = Vector3.Dot(transform.position.normalized, pointToPos.normalized);
                
                Debug.Log(dotProduct);

                if (dotProduct > 0)
                {
                    if (wantedMoveDirection == RopeObject.MoveForward)
                    {
                        Debug.Log("F");
                        wantedMoveDirection = Vector3.zero;
                        currentMoveDirection = Vector3.zero;
                    }
                }
                else
                {
                    if (wantedMoveDirection == -RopeObject.MoveForward)
                    {
                        Debug.Log("B");
                        wantedMoveDirection = Vector3.zero;
                        currentMoveDirection = Vector3.zero;
                    }
                }
            }


            // float startCheck = MathF.Abs((RopeObject.StartPoint - transform.position).magnitude);
            // float endCheck = MathF.Abs((RopeObject.EndPoint - transform.position).magnitude);

            // Debug.Log("Start: " + startCheck);
            // Debug.Log("End: " + endCheck);


            // // Limiting so the player cant go over or under the climbing object 
            // if (startCheck < 1)
            // {
            //     Vector3 otherPos = Vector3.Normalize(RopeObject.StartPoint - transform.position);
            //     float dotProduct = Vector3.Dot(transform.TransformDirection(transform.forward), otherPos);
            //     if (dotProduct > 0)
            //     {
            //         if (wantedMoveDirection.x > 0)
            //         {
            //             wantedMoveDirection.x = 0;
            //             currentMoveDirection.x = 0;
            //         }
            //     }
            //     else
            //     {
            //         if (wantedMoveDirection.x < 0)
            //         {
            //             wantedMoveDirection.x = 0;
            //             currentMoveDirection.x = 0;
            //         }
            //     }
            // }
            // else if (endCheck < 1)
            // {
            //     Vector3 otherPos = Vector3.Normalize(RopeObject.EndPoint - transform.position);
            //     float dotProduct = Vector3.Dot(transform.TransformDirection(transform.forward), otherPos);
            //     if (dotProduct > 0)
            //     {
            //         if (wantedMoveDirection.x > 0)
            //         {
            //             wantedMoveDirection.x = 0;
            //             currentMoveDirection.x = 0;
            //         }
            //     }
            //     else
            //     {
            //         if (wantedMoveDirection.x < 0)
            //         {
            //             wantedMoveDirection.x = 0;
            //             currentMoveDirection.x = 0;
            //         }
            //     }
            // }

            if (wantedMoveDirection == -RopeObject.MoveForward)
            {
                transform.forward = -RopeObject.MoveForward;
            }
            else if (wantedMoveDirection == RopeObject.MoveForward)
            {
                transform.forward = RopeObject.MoveForward;
            }

            finalForce = MoveToWantedPoint() * Time.fixedDeltaTime;


            characterController.Move(finalForce);

        }

        private void AttachStandingPoint()
        {
            canMove = false;
            characterController.enabled = false;

            transform.position = StandingPointObject.SetPlayerAtStandingPoint(BottomPos);

            canMove = true;
            characterController.enabled = true;
        }

        private void StandingPointMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            if (!canMove)
            {
                return;
            }
            else if (StandingPointObject == null)
            {
                MoveType = MoveType.Normal;
                return;
            }
            else if (JumpStage == JumpStage.CommitJump)
            {
                CommitJump();
                MoveType = MoveType.Normal;
                return;
            }
        }

        private void AttachClimbable()
        {
            canMove = false;
            characterController.enabled = false;

            Vector3 point = ClimbableObject.GetClosestPointOnSegment(transform.position);
            transform.position = point + new Vector3(characterController.radius, 0, characterController.radius);
            transform.LookAt(point);

            canMove = true;
            characterController.enabled = true;
        }

        private void ClimbingMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            if (!canMove)
            {
                return;
            }
            else if (ClimbableObject == null)
            {
                MoveType = MoveType.Normal;
                return;
            }
            else if (JumpStage == JumpStage.CommitJump)
            {
                CommitJump();
                MoveType = MoveType.Normal;
                return;
            }

            if (JumpStage != JumpStage.CanJump && JumpStage != JumpStage.CommitJump)
            {
                JumpStage = JumpStage.CanJump;
            }

            if (inDirection != Vector2.zero)
            {
                currentMoveDirection.y = inDirection.y;
                currentMoveDirection.z = inDirection.x;
            }
            else
            {
                currentMoveDirection = Vector3.zero;
            }

            if (currentMoveDirection != Vector3.zero)
            {
                if (currentVelocity < climbingInfo.ClimbSpeed)
                {
                    currentVelocity += climbingInfo.ClimbSpeed * (Time.fixedDeltaTime / climbingInfo.ClimbAccelerationTime);
                }
                else
                {
                    currentVelocity = climbingInfo.ClimbSpeed;
                }
            }
            else
            {
                if (currentVelocity > 0)
                {
                    currentVelocity -= climbingInfo.ClimbSpeed * (Time.fixedDeltaTime / climbingInfo.ClimbAccelerationTime);
                }
                else
                {
                    currentVelocity = 0;
                }
            }

            // Moving left and right 
            if (currentMoveDirection.z != 0)
            {
                characterController.enabled = false;

                Vector3 pivot = ClimbableObject.GetClosestPointOnSegment(transform.position);
                transform.position = Quaternion.Euler(0, climbingInfo.ClimbAroundSpeed * (currentMoveDirection.z * -1), 0) * (transform.position - pivot) + pivot;
                currentMoveDirection.z = 0;

                transform.LookAt(pivot);

                characterController.enabled = true;
            }

            finalForce = (currentMoveDirection * currentVelocity) * Time.fixedDeltaTime;

            float startCheck = Mathf.Abs(ClimbableObject.StartPoint.y - transform.position.y);
            float endCheck = Mathf.Abs(ClimbableObject.EndPoint.y - transform.position.y);

            // Limiting so the player cant go over or under the climbing object 
            if (startCheck < 0.5)
            {
                Vector3 otherPos = Vector3.Normalize(ClimbableObject.StartPoint - transform.position);
                float dotProduct = Vector3.Dot(transform.TransformDirection(Vector3.up), otherPos);
                if (dotProduct > 0)
                {
                    if (finalForce.y > 0)
                    {
                        finalForce.y = 0;
                    }
                }
                else
                {
                    if (finalForce.y < 0)
                    {
                        finalForce.y = 0;
                    }
                }
            }
            else if (endCheck < 0.5)
            {
                Vector3 otherPos = Vector3.Normalize(ClimbableObject.EndPoint - transform.position);
                float dotProduct = Vector3.Dot(transform.TransformDirection(Vector3.up), otherPos);
                if (dotProduct > 0)
                {
                    if (finalForce.y > 0)
                    {
                        finalForce.y = 0;
                    }
                }
                else
                {
                    if (finalForce.y < 0)
                    {
                        finalForce.y = 0;
                    }
                }
            }

            characterController.Move(finalForce);
        }


        // private void RopeMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        // {
        //     if (MagnetObject == null)
        //     {
        //         MoveType = MoveType.Normal;
        //         return;
        //     }

        //     if (!canMove)
        //     {
        //         return;
        //     }

        //     // Input direction
        //     Vector3 moveVector = new Vector3(inDirection.x, 0, inDirection.y).normalized;

        //     // move in the direction of player input
        //     if (moveVector != Vector3.zero)
        //     {

        //         // Face direction of input based on camera forward
        //         // targetRotation = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        //         // transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

        //         Accelerate();
        //     }
        //     else
        //     {
        //         Decelerate();
        //     }

        //     Vector3 targetMoveDirection = MagnetObject.MoveForward * moveVector.z;
        //     //Vector3 targetMoveDirection = (Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward).normalized;

        //     Vector3 finalMoveVector = (targetMoveDirection * currentVelocity + movingGroundInfo.MoveVector) * Time.fixedDeltaTime;

        //     characterController.Move(finalMoveVector);
        // }

    }

}




// Old Movement code, might delete later 😏
/*

 // old movement debugging
                // else if (moveType == MoveType.TestNormal)
                // {
                //     Vector3 forceDirection = new Vector3(moveTargetPoint.x + transform.position.x, transform.position.y, moveTargetPoint.z + transform.position.z);
                //     Vector3 moveDirection = new Vector3(effectMoveTarget.x + transform.position.x, 0, effectMoveTarget.z + transform.position.z) * moveStats.MaxSpeed;
                //     moveDirection.y = transform.position.y;

                //     Gizmos.color = new Color(0.5f, 0f, 0f, 1f);
                //     Gizmos.DrawSphere(forceDirection, 0.6f);
                //     Gizmos.DrawLine(transform.position, forceDirection);

                //     Gizmos.color = new Color(0f, 0.5f, 0f, 1f);
                //     Gizmos.DrawLine(transform.position, moveDirection);
                //     Gizmos.DrawSphere(moveDirection, 0.6f);
                // }


 private void TestNormalMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            if (jumpStage == JumpStage.CommitJump || jumpStage == JumpStage.CommitDoubleJump)
            {
                CommitJump();
            }

            if (!isOnSlope)
            {
                // reset hit slope velocity when grounded
                hitNormal = Vector3.zero;
                externalVector.x = 0;
                externalVector.z = 0;
            }
            else
            {
                // Check for slope
                if (hitNormal != Vector3.zero)
                {
                    CalculateSlopeVector();
                    currentVelocity = 0;
                    hitNormal = Vector3.zero;
                    CanGlide = false;
                }
            }

            if (characterController.isGrounded && !isOnSlope)
            {
                WhenPlayerGrounded();
                CanGlide = false;
            }
            else if (currentCoyoteTime <= moveStats.MaxCoyoteTime)
            {
                currentCoyoteTime += Time.fixedDeltaTime;
                CanGlide = true;
            }
            else if (jumpStage != JumpStage.Reset && jumpStage != JumpStage.CanDoubleJump)
            {
                JumpStage = JumpStage.CanDoubleJump;
                CanGlide = true;
            }

            // Input direction
            Vector3 moveVector = new Vector3(inDirection.x, 0, inDirection.y).normalized;

            if (moveVector != Vector3.zero)
            {
                // Face direction of input based on camera forward
                targetRotation = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

                Accelerate();
            }
            else
            {
                Decelerate();
            }

            effectMoveTarget = (Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward).normalized;

            ApplyGravity();

            effectMoveTarget *= currentVelocity;

            moveTargetPoint = effectMoveTarget;

            // add external vectors
            Vector3 finalMoveVector = (moveTargetPoint + movingGroundInfo.MoveVector + externalVector) * Time.fixedDeltaTime;

            characterController.Move(finalMoveVector);

            effectMoveTarget = Vector3.zero; // temp

            // is the angel of the ground lower then the slop limit
            isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= characterController.slopeLimit;
        }

        private void ApplyGravity()
        {
            float downVelocity;

            float newGravityScale = (2 * moveStats.JumpHeight) / (moveStats.TimeToApex * moveStats.TimeToApex);
            gravityScale = newGravityScale;

            if (!characterController.isGrounded && externalVector.y < 0f)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier;
                downVelocity = moveStats.GravityValue;
            }
            else if (!PressingJump && externalVector.y > 0.05f && jumpStage == JumpStage.CanDoubleJump)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier * moveStats.VariableJumpGravityIncrease;
                downVelocity = moveStats.GravityValue;
            }
            else
            {
                gravityScale = gravityScale * 1;
                downVelocity = moveStats.GravityValue * 1;
            }

            if (externalVector.y >= moveStats.MaxFallSpeed)
            {
                externalVector.y += downVelocity * gravityScale * Time.fixedDeltaTime;
            }
            else
            {
                externalVector.y = moveStats.MaxFallSpeed;
            }

        }

        // Jump code 
            if (jumpStage == JumpStage.CommitJump || moveType == MoveType.OnRope)
                    {
                        if (moveType != MoveType.Normal)
                        {
                            MoveType = MoveType.Normal;
                        }

                        externalVector.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight);
                        JumpStage = JumpStage.CanDoubleJump;
                    }
                    else if (jumpStage == JumpStage.CommitDoubleJump)
                    {
                        externalVector.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight) * moveStats.DoubleJumpEffect;
                        JumpStage = JumpStage.Reset;
                    }

                            private void Accelerate()
        {
            if (currentVelocity < moveStats.MaxSpeed)
            {
                currentVelocity += moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.AccelerationTime);
            }
            else
            {
                currentVelocity = moveStats.MaxSpeed;
            }
        }

        private void Decelerate()
        {
            if (currentVelocity > 0f)
            {

                currentVelocity -= moveStats.MaxSpeed * (Time.fixedDeltaTime / moveStats.DecelerationTime);
            }
            else
            {
                currentVelocity = 0f;
            }
        }


  private void WhenPlayerGrounded()
        {
            gravityScale = 1f;
            currentCoyoteTime = 0;
            if (externalVector.y < 0.1f && jumpStage != JumpStage.CommitJump)
            {
                JumpStage = JumpStage.CanJump;
            }

            // Check if there is moving ground under the player
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayerMask))
            {
                switch (LayerMask.LayerToName(hit.transform.gameObject.layer))
                {
                    case "MovingObject":

                        MovingObject movingObject = hit.transform.GetComponent<MovingObject>();
                        if (movingObject != null)
                        {
                            movingGroundInfo = new(movingObject.CurrentVelocity, movingObject.CurrentMoveDirection + new Vector3(0, 0.5f, 0));
                        }

                        break;
                    case "ConveyorBelt":

                        ConveyorBelt conveyorBelt = hit.transform.GetComponent<ConveyorBelt>();
                        if (conveyorBelt != null)
                        {
                            movingGroundInfo = new(conveyorBelt.CurrentVelocity, conveyorBelt.CurrentMoveDirection + new Vector3(0, 0.5f, 0));
                        }
                        break;
                }

            }
            else if (movingGroundInfo.MoveVector != Vector3.zero)
            {
                movingGroundInfo = new(0, Vector3.zero);
            }

            if (externalVector.y < -2f)
            {
                externalVector.y = -2f;
            }
        }


*/
