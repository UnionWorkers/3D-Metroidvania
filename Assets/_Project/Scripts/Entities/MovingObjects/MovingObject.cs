using Entities;
using Managers;
using UnityEngine;

public class MovingObject : BaseEntity
{
    private float currentVelocity = 0f;
    private Vector3 currentMoveDirection = Vector3.zero;
    private Vector3 desiredMovePoint = Vector3.zero;
    private float movePoint = 0f;


    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxVelocity;
    [SerializeField] private float moveDistance;
    [SerializeField] private Vector3 moveDirection;
    private float realMaxVelocity => maxVelocity * GameManager.Instance.ObjectsGameSpeed;

    public float CurrentVelocity => currentVelocity;
    public Vector3 CurrentMoveDirection => currentMoveDirection;

    public override void OnInitialize()
    {
        gameObject.layer = LayerMask.NameToLayer("MovingObject");
        moveDirection = new Vector3(Mathf.Clamp01(moveDirection.x), Mathf.Clamp01(moveDirection.y), Mathf.Clamp01(moveDirection.z));
        currentMoveDirection = moveDirection;
        desiredMovePoint = NewDesiredMovePoint();
    }

    private void OnDrawGizmos()
    {
        if (!debugState) { return; }

        Gizmos.color = new Color(0.5f, 0f, 0f, 1f);
        Gizmos.DrawSphere(desiredMovePoint, 1f);

        Gizmos.color = new Color(0f, 0.5f, 0f, 1f);
        Gizmos.DrawLine(transform.position, desiredMovePoint);

        Gizmos.color = new Color(0f, 0f, 0.5f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + (currentMoveDirection * 2));
    }

    // public override void OnUpdate()
    // {
    //     if (currentVelocity < maxVelocity)
    //     {
    //         currentVelocity += moveSpeed * Time.deltaTime;
    //     }

    //     float dist = Vector3.Distance(transform.position, desiredMovePoint);

    //     if (dist > 0.15f)
    //     {
    //         transform.position += currentMoveDirection * currentVelocity * Time.deltaTime;
    //     }
    //     else
    //     {
    //         currentMoveDirection = -currentMoveDirection;
    //         desiredMovePoint = NewDesiredMovePoint();
    //         currentVelocity = 0;
    //     }
    // }

    public override void OnFixedUpdate()
    {
        if (currentVelocity < realMaxVelocity)
        {
            currentVelocity += moveSpeed * Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
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

    private Vector3 NewDesiredMovePoint()
    {
        return (moveDistance * currentMoveDirection) + transform.position;
    }

}
