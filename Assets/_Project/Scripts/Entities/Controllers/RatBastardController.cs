using Entities;
using Managers;
using UnityEngine;
using Utils.Triggers;

[System.Serializable]
public struct SpotlightMove
{
    [SerializeField] Transform movingObject;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxVelocity;
    [SerializeField] private float moveDistance;
    [SerializeField] private Vector3 moveDirection;

    public Transform MovingObject => movingObject;
    public float MoveSpeed => moveSpeed;
    public float MoveDistance => moveDistance;
    public Vector3 MoveDirection => new Vector3(Mathf.Clamp01(moveDirection.x), Mathf.Clamp01(moveDirection.y), Mathf.Clamp01(moveDirection.z));
    public float RealMaxVelocity => maxVelocity * GameManager.Instance.ObjectsGameSpeed;

}

public class RatBastardController : BaseEntity
{
    [SerializeField] private HealthTriggerCollision panelTrigger;

    [Space(15)]
    [Header("Spotlight variables")]
    [SerializeField] private TriggerCollisionMessenger visionConeTrigger;
    [SerializeField] SpotlightMove spotlightMove;

    [Space(15)]
    [Header("Shoot variables")]
    [SerializeField] private DamageStruct damageStruct;
    [SerializeField] private float shootCooldownTimer = 2f;
    [SerializeField] private float targetTimerToShoot = 0.5f;

    [Space(15)]
    [Header("Rat hole")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform ratHole = null;


    // Shoot variables
    private float currentShootTimer = 0;
    private float currentTargetTimerToShoot = 0;
    private Transform playerTransform;
    private IHealth targetHealth = null;

    // Move variables
    private float currentVelocity = 0f;
    private Vector3 currentMoveDirection = Vector3.zero;
    private Vector3 desiredMovePoint = Vector3.zero;



    private void Awake()
    {
        visionConeTrigger.OnTriggerCollision += CollisionMessage;
        panelTrigger.OnHealthChanged += PanelHealthChanged;
        panelTrigger.OnDeath += PanelDestroyed;
        lineRenderer.enabled = false;
    }

    public override void OnInitialize()
    {
        currentMoveDirection = spotlightMove.MoveDirection;
        desiredMovePoint = NewDesiredMovePoint();
    }

    private void OnDrawGizmos()
    {
        if (!debugState) { return; }

        Gizmos.color = new Color(0.5f, 0f, 0f, 1f);
        Gizmos.DrawSphere(desiredMovePoint, 1f);

        Gizmos.color = new Color(0f, 0.5f, 0f, 1f);
        Gizmos.DrawLine(spotlightMove.MovingObject.position, desiredMovePoint);

        Gizmos.color = new Color(0f, 0f, 0.5f, 1f);
        Gizmos.DrawLine(spotlightMove.MovingObject.position, spotlightMove.MovingObject.position + (currentMoveDirection * 2));
    }


    public override void OnUpdate()
    {

        if (currentShootTimer < shootCooldownTimer)
        {
            currentShootTimer += Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;
        }
        else
        {
            if (playerTransform != null)
            {
                lineRenderer.SetPosition(0, ratHole.position);
                lineRenderer.SetPosition(1, playerTransform.position);
                lineRenderer.enabled = true;


                if (currentTargetTimerToShoot > targetTimerToShoot)
                {
                    targetHealth.TakeDamage(new(damageStruct.DamageAmount, transform));
                    currentTargetTimerToShoot = 0;
                    currentShootTimer = 0;
                    lineRenderer.enabled = false;
                }
                else
                {
                    currentTargetTimerToShoot += Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;
                }
            }
        }
    }

    public override void OnFixedUpdate()
    {
        if (currentVelocity < spotlightMove.RealMaxVelocity)
        {
            currentVelocity += spotlightMove.MoveSpeed * Time.fixedDeltaTime * GameManager.Instance.ObjectsGameSpeed;
        }

        float dist = Vector3.Distance(spotlightMove.MovingObject.position, desiredMovePoint);

        if (dist > 0.15f)
        {
            spotlightMove.MovingObject.position += currentVelocity * Time.fixedDeltaTime * currentMoveDirection;
        }
        else
        {
            currentMoveDirection = -currentMoveDirection;
            desiredMovePoint = NewDesiredMovePoint();
            currentVelocity = 0;
        }
    }

    private void CollisionMessage(Collider collider, CollisionTriggerType type)
    {
        switch (type)
        {
            case CollisionTriggerType.Enter:
                if (collider.CompareTag("Player"))
                {
                    playerTransform = collider.transform;
                    targetHealth = collider.GetComponent<IHealth>();
                }
                break;
            case CollisionTriggerType.Exit:
                if (collider.CompareTag("Player"))
                {
                    playerTransform = null;
                    targetHealth = null;
                    currentTargetTimerToShoot = 0;
                    lineRenderer.enabled = false;
                }
                break;
        }
    }

    private void PanelDestroyed()
    {
        EntityState = EntityState.Disabled;
        visionConeTrigger.gameObject.SetActive(false);
        lineRenderer.enabled = false;
        panelTrigger.gameObject.SetActive(false);
    }

    private void PanelHealthChanged(int inHealth)
    {
        // add a fun effect
    }

    private Vector3 NewDesiredMovePoint()
    {
        return (spotlightMove.MoveDistance * currentMoveDirection) + spotlightMove.MovingObject.position;
    }

}
