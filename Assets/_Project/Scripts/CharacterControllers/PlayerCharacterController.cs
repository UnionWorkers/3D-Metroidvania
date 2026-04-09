using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

namespace CustomCharacterController
{
    public enum MoveType : byte
    {
        Normal,
        OnRope,
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

        [Header("Time to reach max speed")]
        [Range(0.01f, 0.99f)]
        [SerializeField] private float accelerationTime;

        [Header("Time to reach 0 speed")]
        [Range(0.01f, 0.99f)]
        [SerializeField] private float decelerationTime;
        [SerializeField] private float turnSmoothTime;

        // Air control values
        [Space(15)]
        [Header("Air control values")]

        [Range(0f, 1f)]
        [SerializeField] private float airAccelerateLimiter;

        [Range(0f, 1f)]
        [SerializeField] private float airTurnLimiter;


        [Space(15)]
        [Header("Glide values")]
        [Range(0.001f, 1f)]
        [SerializeField] private float glideGravityLimiter;
        [SerializeField] private float glideMaxFallSpeed;


        public float GravityValue => gravityValue;
        public float GravityMultiplier => gravityMultiplier;
        public float MaxSpeed => maxSpeed;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;
        public float JumpHeight => jumpHeight;
        public float DoubleJumpEffect => doubleJumpEffect;
        public float TimeToApex => timeToApex;
        public float VariableJumpGravityIncrease => variableJumpGravityIncrease;
        // Get a negative value
        public float MaxFallSpeed => Mathf.Abs(maxFallSpeed) * -1;
        public float MaxCoyoteTime => maxCoyoteTime;
        public float AirAccelerateLimiter => airAccelerateLimiter;
        public float AirTurnLimiter => airTurnLimiter;
        public float TurnSmoothTime => turnSmoothTime;

        public float GlideGravityLimiter => glideGravityLimiter;
        public float GlideMaxFallSpeed => Mathf.Abs(glideMaxFallSpeed) * -1;
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

    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] bool debugState = false;
        [SerializeField] private CharacterController characterController;

        [Space(15)]
        [SerializeField] private MoveStats moveStats;
        private float gravityScale = 1f;
        private JumpStage jumpStage = JumpStage.CanJump;

        [Space(15)]
        [SerializeField] private AttackInfo attackInfo;

        // Moving ground variables
        private MovingGroundInfo movingGroundInfo = new(0, Vector3.zero);


        // Slope variables
        [Space(15)]
        [SerializeField] private LayerMask groundLayerMask;
        private bool isOnSlope;
        private Vector3 hitNormal;
        [Header("Slope Variables")]
        [SerializeField] private float slideFriction = 0.3f;
        [SerializeField] private float slidSpeed = 1f;

        // Movement variables
        private float targetRotation = 0;
        private float currentVelocity = 0;
        private Vector3 effectMoveTarget;
        private Vector3 externalVector;
        private Vector3 moveTargetPoint;
        private float currentCoyoteTime = 0;

        // Movement type variables
        [NonSerialized] public MagnetObjectInteractable MagnetObject = null;
        [NonSerialized] public bool CanGlide = false;
        [NonSerialized] public bool PressingJump = false;
        [NonSerialized] public bool LockGlide = false;
        public bool isGround => characterController.isGrounded;
        private MoveType moveType = MoveType.Normal;
        private bool canMove = true;

        [SerializeField] private VisualEffect attackVFX;

        public bool CanMove => canMove;
        public MoveType MoveType
        {
            get => moveType;
            set
            {
                switch (value)
                {
                    case MoveType.Normal:
                        canMove = true;
                        MagnetObject = null;
                        break;
                    case MoveType.OnRope:
                        AttachToRope();
                        break;
                }

                moveType = value;
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


        private void OnDrawGizmos()
        {
            if (debugState)
            {
                if (moveType == MoveType.Normal)
                {

                    Vector3 forceDirection = new Vector3(moveTargetPoint.x + transform.position.x, transform.position.y, moveTargetPoint.z + transform.position.z);
                    Vector3 moveDirection = new Vector3(effectMoveTarget.x + transform.position.x, 0, effectMoveTarget.z + transform.position.z) * moveStats.MaxSpeed;
                    moveDirection.y = transform.position.y;

                    Gizmos.color = new Color(0.5f, 0f, 0f, 1f);
                    Gizmos.DrawSphere(forceDirection, 0.6f);
                    Gizmos.DrawLine(transform.position, forceDirection);

                    Gizmos.color = new Color(0f, 0.5f, 0f, 1f);
                    Gizmos.DrawLine(transform.position, moveDirection);
                    Gizmos.DrawSphere(moveDirection, 0.6f);
                }
                else if (moveType == MoveType.TestNormal)
                {
                    Vector3 wantedMoveDir = (t_wantedMoveDirection * t_wantedVelocity) + transform.position;
                    Vector3 moveDirection = (t_currentMoveDirection * t_currentVelocity) + transform.position;
                    Vector3 externalDir = (t_externalForces) + transform.position;
                    Vector3 finalDir = (t_finalForce * 2) + transform.position;

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

            characterController.stepOffset = 0;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            hitNormal = hit.normal;
        }

        public void MovePlayer(Vector2 inDirection, Transform cameraTransform)
        {
            switch (moveType)
            {
                case MoveType.Normal:
                    NormalMovement(ref inDirection, ref cameraTransform);
                    break;
                case MoveType.OnRope:
                    RopeMovement(ref inDirection, ref cameraTransform);
                    break;

                case MoveType.TestNormal:
                    TestNormalMovement(ref inDirection, ref cameraTransform);
                    break;
            }
        }

        private void NormalMovement(ref Vector2 inDirection, ref Transform cameraTransform)
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


        // Test Variables
        private float t_targetRotation;
        private float t_currentVelocity;
        private Vector3 t_currentMoveDirection;
        private float t_wantedVelocity = 10;
        private Vector3 t_wantedMoveDirection;
        private Vector3 t_externalForces;
        private Vector3 t_slopeForce;
        private Vector3 t_finalForce;
        [Header("Test Variables")]
        [SerializeField] private float t_smoothTurnTurning = 2f;
        [SerializeField] private float t_stepOffset = 0.5f;

        private void TestNormalMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            // Input handling
            if (inDirection != Vector2.zero)
            {
                t_wantedMoveDirection = Quaternion.Euler(0, Mathf.Atan2(inDirection.x, inDirection.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y, 0) * Vector3.forward;
                t_wantedMoveDirection = t_wantedMoveDirection.normalized;
            }
            else
            {
                t_wantedMoveDirection.x = 0;
                t_wantedMoveDirection.z = 0;
            }

            // jump
            if (jumpStage == JumpStage.CommitJump || jumpStage == JumpStage.CommitDoubleJump)
            {
                CommitJump();
            }

            if (!isOnSlope)
            {
                // reset hit slope velocity when grounded
                hitNormal = Vector3.zero;
                t_slopeForce = Vector3.zero;
            }
            else
            {
                // Check for slope
                if (hitNormal != Vector3.zero)
                {
                    CalculateSlopeVector();
                    hitNormal = Vector3.zero;
                    CanGlide = false;
                    LockGlide = false;
                }
            }

            // When is grounded
            if (characterController.isGrounded && !isOnSlope)
            {
                T_WhenPlayerGrounded();
                CanGlide = false;
                LockGlide = false;
            }
            // Coyote
            else if (currentCoyoteTime <= moveStats.MaxCoyoteTime)
            {
                currentCoyoteTime += Time.fixedDeltaTime;
                CanGlide = true;
            }
            // can double jump
            else if (jumpStage != JumpStage.Reset && jumpStage != JumpStage.CanDoubleJump)
            {
                JumpStage = JumpStage.CanDoubleJump;
                CanGlide = true;
            }

            T_ApplyGravity();

            t_finalForce = (T_MoveTOWantedPoint() + t_externalForces) * Time.fixedDeltaTime;

            // step up check when moving 
            bool canStepUp = false;
            if (characterController.isGrounded && inDirection != Vector2.zero)
            {
                canStepUp = T_CanStepUp();
            }

            // add slope force
            if (!canStepUp && isOnSlope)
            {
                t_finalForce += t_slopeForce * Time.fixedDeltaTime;
            }

            // add Rotation to player

            characterController.Move(t_finalForce);

            // is the angel of the ground lower then the slop limit
            isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= characterController.slopeLimit;
        }

        private bool T_CanStepUp()
        {
            Vector3 normalizedMoveDirXZ = new Vector3(t_finalForce.x, 0f, t_finalForce.z).normalized;
            float distance = characterController.radius + characterController.skinWidth;

            Vector3 bottom = transform.position - new Vector3(0f, characterController.height / 2f - characterController.center.y, 0f);
            Vector3 stepOffsetLimit = new(bottom.x, bottom.y + t_stepOffset, bottom.z);
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

        private Vector3 T_MoveTOWantedPoint()
        {
            t_currentMoveDirection = Vector3.Lerp(t_currentMoveDirection, t_wantedMoveDirection, Time.fixedDeltaTime * t_smoothTurnTurning);

            if (t_currentVelocity < t_wantedVelocity && t_wantedMoveDirection != Vector3.zero)
            {
                t_currentVelocity += Time.fixedDeltaTime * 5;
            }
            else if (t_currentVelocity > 0 && t_wantedMoveDirection == Vector3.zero)
            {
                t_currentVelocity -= Time.fixedDeltaTime * 5;
            }

            return t_currentVelocity * t_currentMoveDirection;
        }

        private void AttachToRope()
        {
            canMove = false;
            characterController.enabled = false; // fixed so the player gets set in the right position;

            transform.position = MagnetObject.GetClosestPointOnSegment(transform.position);

            canMove = true;
            characterController.enabled = true;
        }

        // Fix so the player moves with the camera forward
        private void RopeMovement(ref Vector2 inDirection, ref Transform cameraTransform)
        {
            if (MagnetObject == null)
            {
                MoveType = MoveType.Normal;
                return;
            }

            if (!canMove)
            {
                return;
            }

            // Input direction
            Vector3 moveVector = new Vector3(inDirection.x, 0, inDirection.y).normalized;

            // move in the direction of player input
            if (moveVector != Vector3.zero)
            {

                // Face direction of input based on camera forward
                // targetRotation = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                // transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

                Accelerate();
            }
            else
            {
                Decelerate();
            }

            Vector3 targetMoveDirection = MagnetObject.MoveForward * moveVector.z;
            //Vector3 targetMoveDirection = (Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward).normalized;

            Vector3 finalMoveVector = (targetMoveDirection * currentVelocity + movingGroundInfo.MoveVector) * Time.fixedDeltaTime;

            characterController.Move(finalMoveVector);
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

        public void CommitJump()
        {
            switch (moveType)
            {
                case MoveType.Normal:

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

                    break;


                case MoveType.TestNormal:

                    if (jumpStage == JumpStage.CommitJump || moveType == MoveType.OnRope)
                    {
                        if (moveType != MoveType.TestNormal)
                        {
                            MoveType = MoveType.TestNormal;
                        }

                        t_externalForces.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight);
                        JumpStage = JumpStage.CanDoubleJump;
                    }
                    else if (jumpStage == JumpStage.CommitDoubleJump)
                    {
                        t_externalForces.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight) * moveStats.DoubleJumpEffect;
                        JumpStage = JumpStage.Reset;
                    }

                    break;
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

        // when the player is on a slope
        private void CalculateSlopeVector()
        {
            switch (moveType)
            {
                case MoveType.Normal:

                    externalVector.x += (1f - hitNormal.y) * hitNormal.x * (slidSpeed - slideFriction);
                    externalVector.z += (1f - hitNormal.y) * hitNormal.z * (slidSpeed - slideFriction);

                    break;

                case MoveType.TestNormal:

                    t_slopeForce.x += (1f - hitNormal.y) * hitNormal.x * (slidSpeed - slideFriction);
                    t_slopeForce.z += (1f - hitNormal.y) * hitNormal.z * (slidSpeed - slideFriction);

                    break;
            }
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

        private void AirAccelerate()
        {

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
        private void AirDecelerate()
        {

        }

        private void T_WhenPlayerGrounded()
        {
            gravityScale = 1f;
            currentCoyoteTime = 0;
            if (t_wantedMoveDirection.y < 0.1f && jumpStage != JumpStage.CommitJump)
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

            if (t_wantedMoveDirection.y < -2f)
            {
                t_wantedMoveDirection.y = -2f;
            }
        }

        private void T_ApplyGravity()
        {
            float downVelocity;

            float newGravityScale = (2 * moveStats.JumpHeight) / (moveStats.TimeToApex * moveStats.TimeToApex);
            gravityScale = newGravityScale;

            if (!characterController.isGrounded && t_externalForces.y < 0f)
            {
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
            else if (!PressingJump && t_externalForces.y > 0.05f && jumpStage == JumpStage.CanDoubleJump)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier * moveStats.VariableJumpGravityIncrease;
                downVelocity = moveStats.GravityValue;
            }
            else
            {
                gravityScale = gravityScale * 1;
                downVelocity = moveStats.GravityValue;
            }

            if (LockGlide)
            {
                if (t_externalForces.y >= moveStats.GlideMaxFallSpeed)
                {
                    t_externalForces.y += downVelocity * gravityScale * Time.fixedDeltaTime;
                }
                else
                {
                    t_externalForces.y = moveStats.GlideMaxFallSpeed;
                }
            }
            else
            {
                if (t_externalForces.y >= moveStats.MaxFallSpeed)
                {
                    t_externalForces.y += downVelocity * gravityScale * Time.fixedDeltaTime;
                }
                else
                {
                    t_externalForces.y = moveStats.MaxFallSpeed;
                }
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

            if (visualEffect != null)
            {
                Destroy(visualEffect.gameObject, 1f);
            }
        }

        public void CommitDash()
        {

        }
    }

}
