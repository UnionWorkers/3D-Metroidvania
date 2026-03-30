using Entities;
using UnityEngine;

public class RatBastardController : BaseEntity
{
    [SerializeField] private TriggerCollisionMessenger collisionMessenger;
    [SerializeField] private DamageStruct damageStruct;
    [SerializeField] private float shootCooldownTimer = 2f;
    [SerializeField] private float targetTimerToShoot = 0.5f;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform startPoint = null;
    private float currentShootTimer = 0;
    private float currentTargetTimerToShoot = 0;
    private Transform playerTransform;
    private IHealth targetHealth = null;

    private void Awake()
    {
        collisionMessenger.OnTriggerCollision += CollisionMessage;
        lineRenderer.enabled = false;

    }

    public override void OnUpdate()
    {

        if (currentShootTimer < shootCooldownTimer)
        {
            currentShootTimer += Time.deltaTime;
        }
        else
        {
            if (playerTransform != null)
            {
                lineRenderer.SetPosition(0, startPoint.position);
                lineRenderer.SetPosition(1, playerTransform.position);
                lineRenderer.enabled = true;


                if (currentTargetTimerToShoot > targetTimerToShoot)
                {
                    targetHealth.TakeDamage(damageStruct.DamageAmount);
                    currentTargetTimerToShoot = 0;
                    currentShootTimer = 0;
                    lineRenderer.enabled = false;
                }
                else
                {
                    currentTargetTimerToShoot += Time.deltaTime;
                }
            }
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

}
