using CustomCharacterController;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 15;


    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCharacterController PlayerRef = other.transform.GetComponent<PlayerCharacterController>();
            PlayerRef.AddForce(Vector3.up * jumpForce, ForceSource.JumpPad);
        }
    }
}
