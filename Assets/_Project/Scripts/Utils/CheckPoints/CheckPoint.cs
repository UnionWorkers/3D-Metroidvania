using System;
using Entities.Controller;
using UnityEngine;

namespace Utils.Checkpoint
{

    public enum RespawnType : byte
    {
        FullRespawn,
        CheckpointRespawn
    }

    public class CheckPoint : MonoBehaviour
    {
        public Vector3 SpawnPoint; 

        public virtual void SpawnPlayer(ref PlayerController inPlayerController)
        {
            inPlayerController.gameObject.SetActive(false);
            inPlayerController.transform.position = SpawnPoint;
            inPlayerController.gameObject.SetActive(true);
        }

        protected void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                OnTriggerEffect(collider);
            }
        }

        protected virtual void OnTriggerEffect(Collider collider)
        {
            PlayerController playerController = collider.GetComponent<PlayerController>();
            playerController.CheckPoint = this;
        }

    }

}