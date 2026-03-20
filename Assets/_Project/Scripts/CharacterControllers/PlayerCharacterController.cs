using System;
using UnityEngine;

namespace CustomCharacterController
{
    [System.Serializable]
    public struct MoveStats
    {
        [SerializeField] private float gravityValue;

        [SerializeField] private float maxSpeed;
        [Range(0f, 0.99f)]
        [SerializeField] private float accelerationTime;

        [Range(0f, 0.99f)]
        [SerializeField] private float decelerationTime;

        [SerializeField] private float jumpHeight;

        public float GravityValue => gravityValue;
        public float MaxSpeed => maxSpeed;
        public float JumpHeight => jumpHeight;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;

        public float JumpPower => Mathf.Sqrt(jumpHeight * -1 * gravityValue);
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
        [SerializeField] private CharacterController characterController;
        private MovingGroundInfo movingGroundInfo = new(0, Vector3.zero);

        private bool isGrounded;
        private Vector3 hitNormal;
        [SerializeField] private float slideFriction = 0.3f;
        [SerializeField] private float slidSpeed = 1f;
        [SerializeField] private LayerMask groundLayerMask;

        private float currentVelocityTime = 0;
        // find better name? 
        private Vector3 currentMoveVector;

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

        public void Jump()
        {
            if (isGrounded)
            {
                currentMoveVector.y = moveStats.JumpPower;
            }
        }

        public void MovePlayer(Vector2 inDirection, Transform cameraTransform)
        {
            if (isGrounded)
            {
                // reset hit slope velocity when grounded
                hitNormal = Vector3.zero;
                
                currentMoveVector.x = 0;
                currentMoveVector.z = 0;

                WhenPlayerGrounded();
            }
            else
            {
                // Check if on slope  
                if (hitNormal != Vector3.zero)
                {
                    CalculateSlopeVector();
                    hitNormal = Vector3.zero;
                }
            }

            Vector3 moveVector = new Vector3(inDirection.x, 0, inDirection.y);
            moveVector = Vector3.ClampMagnitude(moveVector, 1f);

            // move in the direction of player input 
            if (moveVector != Vector3.zero)
            {
                // Have same forward as camera
                transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);

                moveVector = transform.forward * moveVector.z + transform.right * moveVector.x;

                // calculate velocity time
                if (currentVelocityTime < 1f)
                {
                    currentVelocityTime += (1 - moveStats.AccelerationTime);
                }
                else
                {
                    currentVelocityTime = 1f;
                }
            }
            else
            {
                if (currentVelocityTime > 0f)
                {
                    currentVelocityTime -= (1 - moveStats.DecelerationTime);
                }
                else
                {
                    currentVelocityTime = 0f;
                }
            }

            // applying gravity 
            currentMoveVector.y += moveStats.GravityValue * Time.fixedDeltaTime;

            // currentMoveVector.x += (moveStats.MaxSpeed * currentVelocityTime) + currentMoveVector.x;
            // currentMoveVector.z += (moveStats.MaxSpeed * currentVelocityTime) + currentMoveVector.z;

            moveVector += currentMoveVector;

            // add moving ground velocity
            moveVector = ((moveStats.MaxSpeed * moveVector) + movingGroundInfo.MoveVector) * Time.fixedDeltaTime;

            characterController.Move(moveVector);

            // is the angel of the ground lower then the slop limit 
            isGrounded = Vector3.Angle(Vector3.up, hitNormal) <= characterController.slopeLimit;
        }

        private void WhenPlayerGrounded()
        {
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

    }

}
