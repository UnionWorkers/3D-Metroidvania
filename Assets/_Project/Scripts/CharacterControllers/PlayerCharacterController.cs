using UnityEngine;

namespace CustomCharacterController
{
    [System.Serializable]
    public struct MoveStats
    {
        [SerializeField] private float gravityValue;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpHeight;

        public float GravityValue => gravityValue;
        public float MoveSpeed => moveSpeed;
        public float JumpHeight => jumpHeight;
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

        private Vector3 playerVelocity;
        [SerializeField] private LayerMask groundLayerMask;

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

        public void Jump()
        {
            if (characterController.isGrounded)
            {
                playerVelocity.y = moveStats.JumpPower;
            }
        }

        public void MovePlayer(Vector2 inDirection, Transform cameraTransform)
        {
            if (characterController.isGrounded)
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


                if (playerVelocity.y < -2f)
                {
                    playerVelocity.y = -2f;
                }
            }

            Vector3 move = new Vector3(inDirection.x, 0, inDirection.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (move != Vector3.zero)
            {
                transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
                move = transform.forward * move.z + transform.right * move.x;
            }

            // Apply moving ground movement on the player 

            // applying gravity 
            playerVelocity.y += moveStats.GravityValue * Time.fixedDeltaTime;

            move.y += playerVelocity.y;

            Vector3 finalMove = ((moveStats.MoveSpeed * move) + movingGroundInfo.MoveVector) * Time.fixedDeltaTime;

            characterController.Move(finalMove);
        }
    }

}
