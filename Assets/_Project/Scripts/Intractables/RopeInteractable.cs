using UnityEngine;
using Entities.Controller;
using Interactable;

public class RopeInteractable : BaseInteractable
{
    [SerializeField] Transform startObject = null;
    [SerializeField] Transform endObject = null;

    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;

    public Vector3 MoveForward => (endPoint - startPoint).normalized;
    public float RopeDistance => Vector3.Distance(StartPoint, EndPoint);
    public float DistanceFromStartToPoint(Vector3 point) => Vector3.Distance(StartPoint, point);

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

        if (startObject == null || endObject == null)
        {
            startObject = transform.GetChild(0).transform;
            endObject = transform.GetChild(1).transform;
        }

        if (startObject == null || endObject == null)
        {
            return;
        }

        startObject.position = StartPoint;
        endObject.position = EndPoint;
    }

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        itemState = ItemState.BeingUsed;

        inPlayerController.CharacterController.RopeObject = this;
        inPlayerController.SwitchMoveType(CustomCharacterController.MoveType.OnRope);
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
