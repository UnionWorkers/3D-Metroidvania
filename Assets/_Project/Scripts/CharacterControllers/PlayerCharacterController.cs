using UnityEngine;

[System.Serializable]
public struct MoveStats
{
    public float MoveSpeed;
}

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private MoveStats moveStats;

    [SerializeField] private CharacterController characterController;

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

    }

    public void MovePlayer(Vector2 inDirection)
    {
        Vector3 moveDirection = transform.forward * inDirection.y * moveStats.MoveSpeed;

        characterController.SimpleMove(moveDirection);
    }
}
