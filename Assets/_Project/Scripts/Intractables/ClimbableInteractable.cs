using Entities.Controller;
using Interactable;
using UnityEngine;

public class ClimbableInteractable : BaseInteractable
{
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;

    public Vector3 StartPoint
    {
        get => startPoint + transform.position;
        set => startPoint = value - transform.position;
    }
    public Vector3 EndPoint
    {
        get => endPoint + transform.position;
        set => endPoint = value - transform.position;
    }

    public void Validate()
    {
        
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
        itemState = ItemState.BeingUsed;

        inPlayerController.CharacterController.ClimbableObject = this;
        inPlayerController.SwitchMoveType(CustomCharacterController.MoveType.OnClimbable);
    }

    public Vector3 GetClosestPointOnSegment(Vector3 inPos)
    {
        Vector3 pointToPos = inPos - EndPoint;
        Vector3 endToEnd = StartPoint - EndPoint;

        // is nearest end point 
        float dotProduct = Vector3.Dot(pointToPos, endToEnd);
        if (dotProduct <= 0f) { return EndPoint; }

        pointToPos = inPos - StartPoint;
        endToEnd = EndPoint - StartPoint;

        // is nearest start point 
        dotProduct = Vector3.Dot(pointToPos, endToEnd);
        if (dotProduct <= 0f) { return StartPoint; }

        // point in segment
        return StartPoint + endToEnd.normalized * (dotProduct / endToEnd.magnitude);
    }
}
