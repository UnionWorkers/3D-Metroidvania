using Entities.Controller;
using Interactable;
using Managers;

public class ScorePickUpInteractable : BaseInteractable
{
    protected override void InteractableAction(PlayerController inPlayerController)
    {
        if (itemState == ItemState.Destroyed)
        {
            return;
        }

        itemState = ItemState.Destroyed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScoreToCounter(1);
        }

        Destroy(gameObject);
    }
}
