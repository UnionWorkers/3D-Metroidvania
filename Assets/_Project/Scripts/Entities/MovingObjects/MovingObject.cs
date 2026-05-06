using Entities;
using Managers;
using UnityEngine;

public class MovingObject : BaseEntity
{
    protected float currentVelocity = 0f;
    protected Vector3 currentMoveDirection = Vector3.zero;
    protected Vector3 desiredMovePoint = Vector3.zero;


    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxVelocity;
    [SerializeField] protected float moveDistance;
    [SerializeField] protected Vector3 moveDirection;
    protected virtual float RealMaxVelocity(float gameSpeed) => maxVelocity * gameSpeed;

    public float CurrentVelocity => currentVelocity;
    public Vector3 CurrentMoveDirection => currentMoveDirection;

    protected virtual float Clamp(float clampValue)
    {
        return Mathf.Clamp(clampValue, -1, 1);
    }

    public override void OnInitialize()
    {
        gameObject.layer = LayerMask.NameToLayer("MovingObject");
        moveDirection = new Vector3(Clamp(moveDirection.x), Clamp(moveDirection.y), Clamp(moveDirection.z));
        currentMoveDirection = moveDirection;
        desiredMovePoint = NewDesiredMovePoint();
    }

    private void OnValidate()
    {
        if (!debugState) { return; }

        Validate();
    }

    protected virtual void Validate()
    {
        moveDirection = new Vector3(Clamp(moveDirection.x), Clamp(moveDirection.y), Clamp(moveDirection.z));
        currentMoveDirection = moveDirection;
        desiredMovePoint = NewDesiredMovePoint();
    }

    protected virtual void OnDrawGizmos()
    {
        if (!debugState) { return; }

        Gizmos.color = new Color(0.5f, 0f, 0f, 1f);
        Gizmos.DrawSphere(desiredMovePoint, 1f);

        Gizmos.color = new Color(0f, 0.5f, 0f, 1f);
        Gizmos.DrawLine(transform.position, desiredMovePoint);

        Gizmos.color = new Color(0f, 0f, 0.5f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + (currentMoveDirection * 2));
    }

    public override void OnFixedUpdate(float gameSpeed)
    {
        if (currentVelocity < RealMaxVelocity(gameSpeed))
        {
            currentVelocity += moveSpeed * Time.fixedDeltaTime * gameSpeed;
        }

        float dist = Vector3.Distance(transform.position, desiredMovePoint);

        if (dist > 0.15f)
        {
            transform.position += currentVelocity * Time.fixedDeltaTime * currentMoveDirection;
        }
        else
        {
            currentMoveDirection = -currentMoveDirection;
            desiredMovePoint = NewDesiredMovePoint();
            currentVelocity = 0;
        }
    }

    protected virtual Vector3 NewDesiredMovePoint()
    {
        return (moveDistance * currentMoveDirection) + transform.position;
    }

}
