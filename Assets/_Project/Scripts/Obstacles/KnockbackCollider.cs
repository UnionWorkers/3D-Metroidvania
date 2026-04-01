using UnityEngine;

public class KnockbackCollider : MonoBehaviour
{
    public float KnockbackPower;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter (Collider other)    
    {
        // Debug.Log("Hit spmething");
        // if (other.gameObject.CompareTag("Player"))
        // {
        //     Debug.Log("Hit the PLayer");
        //     CharacterController PlayerRef = other.transform.GetComponent<CharacterController>();
        //     Vector3 BackDirections = this.transform.forward;
        //     PlayerRef.Move(BackDirections * KnockbackPower);
        //   
        // }
        if (other.gameObject.CompareTag("Player"))
        {
            CharacterController PlayerRef = other.transform.GetComponent<CharacterController>();
            Vector3 PlayerPush = this.transform.forward;
            float dotDir = Vector3.Dot(PlayerRef.transform.position, this.transform.position);
            Debug.Log(dotDir);

            if (dotDir < 0)
            {
                PlayerPush = -1 * this.transform.forward;
            }
            else if (dotDir > 0)
            {
                PlayerPush = this.transform.forward;
            }
            PlayerRef.Move(PlayerPush * KnockbackPower);
        }
    }
}
