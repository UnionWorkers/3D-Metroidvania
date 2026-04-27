using Entities.Controller;
using Interactable;
using Interactable.Key;
using UnityEngine;

public enum DoorState : byte
{
    Open,
    Closed,
    Locked,
}

public enum DoorAction : byte
{
    OpenOnce,
    Reusable,
    Inactive
}

public class DoorInteractable : BaseInteractable
{
    [SerializeField] private DoorState doorState = DoorState.Locked;
    [SerializeField] private DoorAction doorAction = DoorAction.OpenOnce;
    [SerializeField] private KeySO keySO;
    private Key key => keySO.Key;

    protected override void Start()
    {
        Validate();
    }

    private bool Validate()
    {
        if (keySO == null) { Debug.LogWarning("Has no KeySO, this items will not work"); return false; }

        if (!keySO.HasKey) { Debug.LogWarning("KeySO Has no key, item will not work"); return false; }

        return true;
    }

    public override void Highlight()
    {
        base.Highlight();
    }

    public override void DeHighlight()
    {
        base.DeHighlight();
    }

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        if (!Validate()) { return; }

        if (doorState == DoorState.Locked && !inPlayerController.Inventory.HasKey(key))
        {
            return;
        }

        switch (doorAction)
        {
            case DoorAction.OpenOnce:
                CommitAction(inPlayerController);
                doorAction = DoorAction.Inactive;
                break;
            case DoorAction.Reusable:
                CommitAction(inPlayerController);
                break;
            case DoorAction.Inactive:
                break;
        }
    }

    protected virtual void CommitAction(PlayerController inPlayerController)
    {
        switch (doorState)
        {
            case DoorState.Open:
                transform.RotateAround(transform.position, transform.rotation.eulerAngles, 40f);
                doorState = DoorState.Closed;
                break;
            case DoorState.Closed:
                transform.RotateAround(transform.position, new Vector3(0, 10, 0), 40f);
                doorState = DoorState.Open;
                break;
            case DoorState.Locked:
                inPlayerController.Inventory.UseKey(key);
                doorState = DoorState.Closed;
                CommitAction(inPlayerController);

                break;
        }


    }

}
