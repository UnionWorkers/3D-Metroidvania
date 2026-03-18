using UnityEngine;
using Utils.Math;

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

        [SerializeField] private EasingFunctionType easingGravityType;
        private Vector3 playerVelocity;
        private float timeInAir = 0f;
        [Range(0, 5)]
        [SerializeField] private float maxAirTimer = 1f;
        [SerializeField] private float maxFallVelocity = 5f;




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

        public void MovePlayer(Vector2 inDirection, Transform cameraTransform)
        {

            if (characterController.isGrounded)
            {
                if (playerVelocity.y < -2f)
                {
                    playerVelocity.y = -2f;
                }
                timeInAir = Time.deltaTime;
            }
            else
            {
                if (timeInAir <= maxAirTimer)
                {
                    timeInAir += Time.deltaTime;
                }
            }

            Vector3 move = new Vector3(inDirection.x, 0, inDirection.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if (move != Vector3.zero)
            {
                transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
                move = transform.forward * move.z + transform.right * move.x;
            }

            // applying gravity 
            if (playerVelocity.y <= maxFallVelocity)
            {
                playerVelocity.y += moveStats.GravityValue * EasingFunctions.EasingFunction(easingGravityType, timeInAir);
            }
            else
            {
                playerVelocity.y = maxFallVelocity;
            }

            move.y += playerVelocity.y;

            Vector3 finalMove = (moveStats.MoveSpeed * move) * Time.deltaTime;

            characterController.Move(finalMove);
        }
    }
}
