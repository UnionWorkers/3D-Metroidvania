using Entities.Controller;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public void SpawnPlayer(PlayerController inPlayerController)
    {
        inPlayerController.gameObject.SetActive(false);
        inPlayerController.transform.position = transform.position;
        inPlayerController.gameObject.SetActive(true);
    }
}
