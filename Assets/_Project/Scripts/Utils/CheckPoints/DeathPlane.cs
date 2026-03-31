using Managers;
using UnityEngine;

namespace Utils.Checkpoint
{
    public class DeathPlane : MonoBehaviour
    {

        private void OnTriggerEffect(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                collider.GetComponent<IHealth>().TakeDamage(1);
                GameManager.Instance.RespawnPlayer(RespawnType.CheckpointRespawn);
            }
        }

    }

}