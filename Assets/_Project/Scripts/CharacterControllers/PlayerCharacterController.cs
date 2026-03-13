using UnityEngine;

namespace CustomCharacterController
{
    [System.Serializable]
    public struct MoveStats
    {
        [SerializeField] private float gravityValue;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpHeight;

        private Camera mainCamera; 

        public float GravityValue => gravityValue;
        public float MoveSpeed => moveSpeed;
        public float JumpHeight => jumpHeight;
        public float JumpPower => Mathf.Sqrt(jumpHeight * -1 * gravityValue);

    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private MoveStats moveStats;
        [SerializeField] private CharacterController characterController;

        private Vector3 playerVelocity;

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

        public void Interact()
        {

        }

        public void Jump()
        {
            if (characterController.isGrounded)
            {
                playerVelocity.y = moveStats.JumpPower;
            }
        }

        public void MovePlayer(Vector2 inDirection, Quaternion inCameraRotation)
        {

            if (characterController.isGrounded)
            {
                if (playerVelocity.y < -2f)
                {
                    playerVelocity.y = -2f;
                }
            }

            Vector3 move = new Vector3(inDirection.x, 0, inDirection.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (move != Vector3.zero)
            {
                transform.forward = move;
            }

            playerVelocity.y += moveStats.GravityValue * Time.deltaTime;

            Vector3 finalMove = (moveStats.MoveSpeed * move + Vector3.up * playerVelocity.y) * Time.deltaTime;

            characterController.Move(finalMove);
        }
    }
}
