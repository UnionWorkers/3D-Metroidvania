using UnityEngine;
using Entities.Controller;
using Interactable;

public class RopeInteractable : BaseInteractable
{
    [SerializeField] protected ParticleSystem interactableVFX2;

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

        inPlayerController.CharacterController.RopeObject = this;
        inPlayerController.SwitchMoveType(CustomCharacterController.MoveType.OnRope);
    }

    // add so it uses the limiter
    public Vector3 GetClosestPointOnSegment(Vector3 inPos, float alphaLimiter)
    {
        Vector3 limitedStart = Vector3.Lerp(StartPoint, EndPoint, alphaLimiter);
        Vector3 limitedEnd = Vector3.Lerp(StartPoint, EndPoint, (1 - alphaLimiter));

        Vector3 pointToPos = inPos - limitedEnd;
        Vector3 endToEnd = limitedStart - limitedEnd;

        // is nearest end point 
        float dotProduct = Vector3.Dot(pointToPos, endToEnd);
        if (dotProduct <= 0f) { return limitedEnd; }

        pointToPos = inPos - limitedStart;
        endToEnd = limitedEnd - limitedStart;

        // is nearest start point 
        dotProduct = Vector3.Dot(pointToPos, endToEnd);
        if (dotProduct <= 0f) { return limitedStart; }

        // point in segment
        return limitedStart + endToEnd.normalized * (dotProduct / endToEnd.magnitude);
    }

}
