using System;
using Unity.Mathematics;
using UnityEngine;

namespace CustomCharacterController
{
    public enum MoveType : byte
    {
        Normal,
        OnRope
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
        [SerializeField] private MoveStats moveStats;
        private float gravityScale = 1f;
        public bool PressingJump = false;
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
        private float currentVelocityTime = 0;
        private Vector3 currentMoveVector; // find better name?
        private float currentCoyoteTime = 0;

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
                currentMoveVector.x = 0;
                currentMoveVector.z = 0;
            }
            else
            {
                // Check for slope 
                if (hitNormal != Vector3.zero)
                {
                    CalculateSlopeVector();
                    currentVelocityTime = 0;
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

            Vector3 targetMoveDirection = (Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward).normalized;

            ApplyGravity();

            // add moving ground velocity
            Vector3 finalMoveVector = (targetMoveDirection * (moveStats.MaxSpeed * currentVelocityTime) + movingGroundInfo.MoveVector + currentMoveVector) * Time.fixedDeltaTime;

            characterController.Move(finalMoveVector);

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


            Vector3 finalMoveVector = (targetMoveDirection * (moveStats.MaxSpeed * currentVelocityTime) + movingGroundInfo.MoveVector) * Time.fixedDeltaTime;

            characterController.Move(finalMoveVector);
        }

        private void ApplyGravity()
        {
            float downVelocity;

            float newGravityScale = (2 * moveStats.JumpHeight) / (moveStats.TimeToApex * moveStats.TimeToApex);
            gravityScale = newGravityScale;

            if (!characterController.isGrounded && currentMoveVector.y < 0f)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier;
                downVelocity = moveStats.GravityValue;
            }
            else if (!PressingJump && currentMoveVector.y > 0.05f && jumpStage == JumpStage.CanDoubleJump)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier * moveStats.VariableJumpGravityIncrease;
                downVelocity = moveStats.GravityValue;
            }
            else
            {
                gravityScale = gravityScale * 1;
                downVelocity = moveStats.GravityValue * 1;
            }

            if (currentMoveVector.y >= moveStats.MaxFallSpeed)
            {
                currentMoveVector.y += downVelocity * gravityScale * Time.fixedDeltaTime;
            }
            else
            {
                currentMoveVector.y = moveStats.MaxFallSpeed;
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

                currentMoveVector.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight);
                JumpStage = JumpStage.CanDoubleJump;
            }
            else if (jumpStage == JumpStage.CommitDoubleJump)
            {
                currentMoveVector.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpHeight) * moveStats.DoubleJumpEffect;
                JumpStage = JumpStage.Reset;
            }
        }

        private void WhenPlayerGrounded()
        {
            gravityScale = 1f;
            currentCoyoteTime = 0;
            if (currentMoveVector.y < 0.1f && jumpStage != JumpStage.CommitJump)
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

            if (currentMoveVector.y < -2f)
            {
                currentMoveVector.y = -2f;
            }
        }

        // when the player is on a slope 
        private void CalculateSlopeVector()
        {
            currentMoveVector.x += (1f - hitNormal.y) * hitNormal.x * (slidSpeed - slideFriction);
            currentMoveVector.z += (1f - hitNormal.y) * hitNormal.z * (slidSpeed - slideFriction);
        }

        private void Accelerate()
        {
            if (currentVelocityTime < 1f)
            {
                currentVelocityTime += Time.fixedDeltaTime / moveStats.AccelerationTime;
            }
            else
            {
                currentVelocityTime = 1f;
            }
        }

        private void Decelerate()
        {
            if (currentVelocityTime > 0f)
            {
                currentVelocityTime -= Time.fixedDeltaTime / moveStats.DecelerationTime;
            }
            else
            {
                currentVelocityTime = 0f;
            }
        }

    }

}
