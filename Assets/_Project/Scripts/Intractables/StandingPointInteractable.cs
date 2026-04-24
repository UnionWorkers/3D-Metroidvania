using System;
using Entities.Controller;
using Interactable;
using UnityEngine;

public class StandingPointInteractable : BaseInteractable
{
    [SerializeField] private Vector3 standingPoint;
    
    [Header("Only use if the position is weird")]
    [SerializeField] private Vector3 offsetPlayer;

    public Vector3 StandingPoint
    {
        get => standingPoint + transform.position;
        set => standingPoint = value - transform.position;
    }

    public Vector3 OffsetPlayer
    {
        get => offsetPlayer;
        set => offsetPlayer = value;
    }

    public void Validate()
    {
        // VFX move thing  
    }

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        itemState = ItemState.BeingUsed;

        inPlayerController.CharacterController.StandingPointObject = this;
        inPlayerController.SwitchMoveType(CustomCharacterController.MoveType.OnStandingPoint);
    }

    public Vector3 SetPlayerAtStandingPoint(float characterHeight)
    { 
        Vector3 pos = StandingPoint + offsetPlayer;
        pos.y += characterHeight; 
        return pos;
    }
}
