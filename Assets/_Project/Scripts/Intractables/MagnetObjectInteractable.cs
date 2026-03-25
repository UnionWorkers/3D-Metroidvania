using UnityEngine;
using Entities.Controller;
using Interactable;

public class MagnetObjectInteractable : BaseInteractable
{
    [SerializeField] LineRenderer lineRenderer = null;
    [SerializeField] Transform startObject = null;
    [SerializeField] Transform endObject = null;

    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;

    public Vector3 MoveForward => (endPoint - startPoint).normalized;


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

    private void OnValidate()
    {
        Validate();
    }

    public void Validate()
    {
        if (startObject == null || endObject == null || lineRenderer == null)
        {
            startObject = transform.GetChild(0).transform;
            endObject = transform.GetChild(1).transform;
            lineRenderer = GetComponent<LineRenderer>();
        }

        Vector3 objectPos = transform.position;

        startObject.position = startPoint + objectPos;
        endObject.position = endPoint + objectPos;

        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, startPoint + objectPos);
        lineRenderer.SetPosition(1, endPoint + objectPos);
    }

    protected override void Start()
    {
        defaultColor = transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
    }

    public override void Highlight()
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
        transform.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);

        itemState = ItemState.Highlighted;
    }

    public override void DeHighlight()
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", defaultColor);
        transform.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", defaultColor);

        itemState = ItemState.None;
    }

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        itemState = ItemState.BeingUsed;

        inPlayerController.CharacterController.MagnetObject = this;
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
