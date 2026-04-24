using Entities.Controller;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public void SpawnPlayer(PlayerController inPlayerController)
    {
        inPlayerController.gameObject.SetActive(false);

        inPlayerController.transform.position = transform.position + new Vector3(0, inPlayerController.CharacterController.HeightValue, 0);

        inPlayerController.gameObject.SetActive(true);
    }
}
