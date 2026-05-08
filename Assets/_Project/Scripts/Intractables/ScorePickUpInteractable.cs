using Entities.Controller;
using Managers;
using UnityEngine;

public class ScorePickUpInteractable : MonoBehaviour
{

    protected ParticleSystem activeVFX;
    [SerializeField] protected ParticleSystem interactVFX;
    [SerializeField] protected ParticleSystem interactableVFX;

    private void Start()
    {
        if (interactableVFX != null && activeVFX == null)
        {
            activeVFX = Instantiate(interactableVFX, transform.position, Quaternion.identity);
            activeVFX.Play();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                InteractableAction(player);
            }
        }

    }

    protected virtual void InteractableAction(PlayerController inPlayerController)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScoreToCounter(1);
        }

        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (activeVFX != null)
        {
            Destroy(activeVFX);
        }
        if (interactVFX != null)
        {
            activeVFX = Instantiate(interactVFX, transform.position, Quaternion.identity);
            activeVFX.Play();
            Destroy(activeVFX, 1f);
        }
    }
}
