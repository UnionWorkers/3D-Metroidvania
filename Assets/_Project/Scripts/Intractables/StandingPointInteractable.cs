using System;
using Entities.Controller;
using Interactable;
using UnityEngine;

public class StandingPointInteractable : BaseInteractable
{
    [SerializeField] private Vector3 standingPoint;
    [SerializeField] private Transform vfxPlayer;


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

    private void OnValidate()
    {
        Validate();
    }

    protected override void Start()
    {
        if (interactableVFX != null)
        {
            interactableVFX.Stop();
        }
    }

    public void Validate()
    {
        if (vfxPlayer == null) { return; }
        
        vfxPlayer.position = StandingPoint;

        ParticleSystem particleSystem = vfxPlayer.GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            Debug.LogWarning("VFXPlayer dose not have a particleSystem");
            interactableVFX = null;
            return;
        }
        interactableVFX = particleSystem;
    }

    public override void Highlight()
    {
        if (interactableVFX != null)
        {
            interactableVFX.Play();
        }
        itemState = ItemState.Highlighted;

    }


    public override void DeHighlight()
    {
        if (interactableVFX != null)
        {
            interactableVFX.Stop();
        }
        itemState = ItemState.None;

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
