using System;
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
        HasJumped,
        CommitDoubleJump,
        Reset
    }



    [System.Serializable]
    public struct MoveStats
    {
        [SerializeField] private float gravityValue;
        [Range(1f, 10f)]
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float jumpHeight;
        [Range(0.2f, 8f)]
        [SerializeField] private float timeToApex;


        [Space(15)]
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
        public float JumpHeight => jumpHeight;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;
        public float JumpPower => jumpHeight;
        public float TimeToApex => timeToApex;

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
        public JumpStage JumpStage = JumpStage.CanJump;


        [Space(15)]
        [SerializeField] private CharacterController characterController;

        // Moving ground variables 
        private MovingGroundInfo movingGroundInfo = new(0, Vector3.zero);
        [SerializeField] private LayerMask groundLayerMask;

        // Slope variables 
        private bool isOnSlope;
        private Vector3 hitNormal;
        [SerializeField] private float slideFriction = 0.3f;
        [SerializeField] private float slidSpeed = 1f;

        // Movement variables 
        private float targetRotation = 0;
        private float currentVelocityTime = 0;
        private Vector3 currentMoveVector; // find better name?

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
            if (JumpStage == JumpStage.CommitJump || JumpStage == JumpStage.CommitDoubleJump)
            {
                CommitJump();
            }

            // if (PressingJump && JumpStage == JumpStage.HasJumped)
            // {
            //     if (currentMoveVector.y > 0.01)
            //     {
            //         currentMoveVector.y *= 1.2f;
            //     }
            // }

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

            float newGravityScale = (2 * moveStats.JumpPower) / (moveStats.TimeToApex * moveStats.TimeToApex);
            gravityScale = newGravityScale;

            if (!characterController.isGrounded && currentMoveVector.y < 0f)
            {
                gravityScale = gravityScale * moveStats.GravityMultiplier;
                downVelocity = moveStats.GravityValue * moveStats.GravityMultiplier;
            }
            else
            {
                gravityScale = gravityScale * 1;
                downVelocity = moveStats.GravityValue * 1;
            }

            currentMoveVector.y += downVelocity * gravityScale * Time.fixedDeltaTime;
        }

        public void CommitJump()
        {
            if (JumpStage == JumpStage.CommitJump)
            {
                if (moveType != MoveType.Normal)
                {
                    MoveType = MoveType.Normal;
                }
                if (characterController.isGrounded || moveType == MoveType.OnRope)
                {
                    currentMoveVector.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpPower);
                    JumpStage = JumpStage.HasJumped;
                }
            }
            else if (JumpStage == JumpStage.CommitDoubleJump)
            {
                currentMoveVector.y = Mathf.Sqrt(-2f * moveStats.GravityValue * gravityScale * moveStats.JumpPower);
                JumpStage = JumpStage.Reset;
            }

            Debug.Log(currentMoveVector.y);
        }

        private void WhenPlayerGrounded()
        {
            gravityScale = 1f;
            if (JumpStage != JumpStage.CommitJump)
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
