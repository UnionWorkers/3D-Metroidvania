using System;
using TreeEditor;
using UnityEngine;

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

    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] bool debugState = false;

        [SerializeField] private MoveStats moveStats;
        private float gravityScale = 1f;
        [NonSerialized] public bool PressingJump = false;
        private JumpStage jumpStage = JumpStage.CanJump;
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

        [Space(15)]
        [SerializeField] private CharacterController characterController;

        // Moving ground variables 
        private MovingGroundInfo movingGroundInfo = new(0, Vector3.zero);
        [SerializeField] private LayerMask groundLayerMask;

        // Slope variables 
        [Space(15)]
        [Header("Slope Variables")]
        private bool isOnSlope;
        private Vector3 hitNormal;
        [SerializeField] private float slideFriction = 0.3f;
        [SerializeField] private float slidSpeed = 1f;

        // Movement variables 
        private float targetRotation = 0;
        private float currentVelocity = 0;
        private Vector3 effectMoveTarget;
        private Vector3 externalVector;
        private Vector3 moveTargetPoint;
        private float currentCoyoteTime = 0;
        private float turnVelocity = 0;

        // Movement type variables 
        [NonSerialized] public MagnetObjectInteractable MagnetObject = null;
        private MoveType moveType = MoveType.Normal;
        private bool canMove = true;
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

        private void OnDrawGizmos()
        {
            if (debugState)
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
                }
            }

            if (characterController.isGrounded && !isOnSlope)
            {
                WhenPlayerGrounded();
            }
            else if (currentCoyoteTime <= moveStats.MaxCoyoteTime)
            {
                currentCoyoteTime += Time.fixedDeltaTime;
            }
            else if (jumpStage != JumpStage.Reset && jumpStage != JumpStage.CanDoubleJump)
            {
                JumpStage = JumpStage.CanDoubleJump;
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
                }
            }

            if (characterController.isGrounded && !isOnSlope)
            {
                WhenPlayerGrounded();
            }
            else if (currentCoyoteTime <= moveStats.MaxCoyoteTime)
            {
                currentCoyoteTime += Time.fixedDeltaTime;
            }
            else if (jumpStage != JumpStage.Reset && jumpStage != JumpStage.CanDoubleJump)
            {
                JumpStage = JumpStage.CanDoubleJump;
            }

            // Input direction
            Vector3 moveVector = new Vector3(inDirection.x, 0, inDirection.y).normalized;

            // move in the direction of player input
            if (characterController.isGrounded)
            {

                if (moveVector != Vector3.zero)
                {
                    // Face direction of input based on camera forward 
                    targetRotation = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                    //transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

                    Accelerate();
                }
                else
                {
                    Decelerate();
                }
            }
            else
            {
                // air controls
                if (moveVector != Vector3.zero)
                {
                    AirAccelerate();
                }
                else
                {
                    AirDecelerate();
                }
            }

            //  effectMoveTarget = (Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward).normalized;
            effectMoveTarget = ((transform.forward * moveVector.z) + (transform.right * moveVector.x)).normalized;

            ApplyGravity();

            if (moveTargetPoint.magnitude < (effectMoveTarget * moveStats.MaxSpeed).magnitude)
            {
                moveTargetPoint += effectMoveTarget.normalized * currentVelocity;
            }
            else if (effectMoveTarget != Vector3.zero)
            {
                moveTargetPoint -= moveTargetPoint.normalized * currentVelocity;
            }

            // add external vectors
            Vector3 finalMoveVector = (moveTargetPoint + movingGroundInfo.MoveVector + externalVector) * Time.fixedDeltaTime;

            // test remove later 
            finalMoveVector.x = 0;
            finalMoveVector.z = 0;

            characterController.Move(finalMoveVector);

            // effectMoveTarget = Vector3.zero; // temp

            // is the angel of the ground lower then the slop limit 
            isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= characterController.slopeLimit;
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
                MovingObject movingObject = hit.transform.GetComponent<MovingObject>();
                if (movingObject != null)
                {
                    movingGroundInfo = new(movingObject.CurrentVelocity, movingObject.CurrentMoveDirection + new Vector3(0, 0.5f, 0));
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
            externalVector.x += (1f - hitNormal.y) * hitNormal.x * (slidSpeed - slideFriction);
            externalVector.z += (1f - hitNormal.y) * hitNormal.z * (slidSpeed - slideFriction);
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


    }

}
