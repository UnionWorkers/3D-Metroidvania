using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharacterController PlayerRef = other.transform.GetComponent<CharacterController>();
            Vector3 PlayerPush = this.transform.up;
            PlayerRef.Move(PlayerPush *  jumpForce);
        }
    }
}
