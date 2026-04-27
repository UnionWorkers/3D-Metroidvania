using Entities.Controller;
using Interactable;
using Interactable.Key;
using UnityEngine;

public class KeyInteractable : BaseInteractable
{
    [UnityEngine.SerializeField] private KeySO keySO;
    public Key GetKey => keySO.Key;

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        if (keySO == null) { Debug.LogWarning("Has no KeySO, this items will not work"); return; }

        if (!keySO.HasKey) { Debug.LogWarning("KeySO Has no key, item will not work"); return; }

        inPlayerController.Inventory.AddKey(GetKey);

        base.InteractableAction(inPlayerController);
    }
}
