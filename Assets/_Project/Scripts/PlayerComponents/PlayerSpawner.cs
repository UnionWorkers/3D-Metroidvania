using Entities.Controller;
using UnityEngine;
using Utils.Checkpoint;

namespace Utils.Checkpoint
{
    public class PlayerSpawner : CheckPoint
    {
        public void FullSpawnPlayer(PlayerController inPlayerController)
        {
            inPlayerController.gameObject.SetActive(false);

            inPlayerController.transform.position = transform.position + new Vector3(0, inPlayerController.CharacterController.HeightValue, 0);
            inPlayerController.CheckPoint = this;

            inPlayerController.gameObject.SetActive(true);
        }
    }

}