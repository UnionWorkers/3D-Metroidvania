using Entities.Controller;
using Interactable;
using Interactable.Key;

public class KeyInteractable : BaseInteractable
{
    [UnityEngine.SerializeField] private KeySO keySO;
    public Key GetKey => keySO.Key;

    protected override void InteractableAction(PlayerController inPlayerController)
    {

        inPlayerController.Inventory.AddKey(GetKey);

        base.InteractableAction(inPlayerController);
    }
}
