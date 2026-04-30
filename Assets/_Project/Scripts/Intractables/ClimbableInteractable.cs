using Entities.Controller;
using Interactable;
using UnityEngine;

public class ClimbableInteractable : BaseInteractable
{
    [SerializeField] protected ParticleSystem interactableVFX2;
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;
    [SerializeField] private Transform startObject;
    [SerializeField] private Transform endObject;


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

    public Vector3 MoveForward => (endPoint - startPoint).normalized;
    public float ClimeDistance => Vector3.Distance(StartPoint, EndPoint);
    public float DistanceFromStartToPoint(Vector3 point) => Vector3.Distance(StartPoint, point);

    private void OnValidate()
    {
        Validate();
    }

    public void Validate()
    {
        if (startObject == null || endObject == null)
        {
            startObject = transform.GetChild(0).transform;
            endObject = transform.GetChild(1).transform;
        }

        if (startObject == null || endObject == null) { return; }

        startObject.position = StartPoint;
        endObject.position = EndPoint;

        if (interactableVFX == null || interactableVFX2 == null)
        {
            ParticleSystem particleSystem = startObject.GetComponent<ParticleSystem>();
            ParticleSystem particleSystem2 = endObject.GetComponent<ParticleSystem>();
            if (particleSystem == null || particleSystem2 == null)
            {
                Debug.LogWarning("VFXPlayerStart or vfxPlayerEnd dose not have a particleSystem");
                interactableVFX = null;
                return;
            }
            interactableVFX = particleSystem;
            interactableVFX2 = particleSystem2;
        }
    }

    protected override void Start()
    {
        if (interactableVFX != null)
        {
            interactableVFX.Stop();
        }
        if (interactableVFX2 != null)
        {
            interactableVFX2.Stop();
        }
    }


    public override void Highlight()
    {
        if (interactableVFX != null && interactableVFX2 != null)
        {
            interactableVFX.Play();
            interactableVFX2.Play();
        }
        itemState = ItemState.Highlighted;
    }


    public override void DeHighlight()
    {
        if (interactableVFX != null && interactableVFX2 != null)
        {
            interactableVFX.Stop();
            interactableVFX2.Stop();
        }
        itemState = ItemState.None;
    }

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        itemState = ItemState.BeingUsed;

        inPlayerController.CharacterController.ClimbableObject = this;
        inPlayerController.SwitchMoveType(CustomCharacterController.MoveType.OnClimbable);
    }


    // add so it uses the limiter
    public Vector3 GetClosestPointOnSegment(Vector3 inPos, float alphaLimiter)
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
