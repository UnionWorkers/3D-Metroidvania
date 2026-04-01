using Managers;
using UnityEngine;

namespace Utils.Checkpoint
{
    public class DeathPlane : MonoBehaviour
    {
        void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                collider.GetComponent<IHealth>().TakeDamage(new(1));
                GameManager.Instance.RespawnPlayer(RespawnType.CheckpointRespawn);
            }
        }

    }

}