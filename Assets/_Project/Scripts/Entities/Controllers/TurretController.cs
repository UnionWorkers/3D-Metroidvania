using System;
using Entities;
using Managers;
using UnityEngine;
using Utils.Triggers;

[ExecuteInEditMode]
public class TurretController : BaseEntity, IHealth
{
    [SerializeField] private Transform poleTransform = null;
    [SerializeField] private TriggerCollisionMessenger visionCone;
    [SerializeField] private HealthComponent healthComponent;
    private Transform playerTransform;
    private IHealth targetHealth = null;

    private float currentRotation = 0;
    //1 = left, -1 = right
    private int rotationDirection = 1;
    private bool canReset = false;


    [Space(15)]
    [Range(-180, 180)]
    [SerializeField] private int minRotationClamp, maxRotationClamp;
    [SerializeField] private float rotationSpeed = 5;

    [Space(15)]
    [SerializeField] private float maxTimerToLoseTarget = 5f;
    private float currentTimerToLoseTarget = 0;
    private Vector3 lastPlayerLocation = Vector3.zero;

    [Space(15)]
    [SerializeField] DamageStruct damageStruct;
    [SerializeField] private float shootTimer = 2f;
    private float currentShootTimer = 0;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        visionCone.OnTriggerCollision += CollisionMessage;
        rotationDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
    }

    public void Heal(int inHealth) { return; }

    public void TakeDamage(DamageInfo inDamageInfo)
    {
        healthComponent.CurrentHealth -= inDamageInfo.DamageAmount;
        OnHealthChanged?.Invoke(healthComponent.CurrentHealth);
        if (healthComponent.CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
            EntityState = EntityState.Disabled;
            visionCone.gameObject.SetActive(false);
        }
    }

    public override void OnUpdate()
    {
        if (playerTransform != null)
        {
            TargetRotate(playerTransform.position);
            ChargingAndAttack();
        }
        else if (lastPlayerLocation != Vector3.zero && currentTimerToLoseTarget <= maxTimerToLoseTarget)
        {
            TargetRotate(lastPlayerLocation);
            currentTimerToLoseTarget += Time.deltaTime;
        }
        else if (poleTransform != null)
        {
            if (canReset)
            {
                currentTimerToLoseTarget = 0f;
                lastPlayerLocation = Vector3.zero;
                currentShootTimer = 0;
                targetHealth = null;
            }

            if (rotationDirection == 0)
            {
                rotationDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            }

            if (currentRotation <= minRotationClamp || currentRotation >= maxRotationClamp)
            {
                rotationDirection *= -1;
            }

            RotateTurret();
        }
    }

    private void TargetRotate(Vector3 targetLocation)
    {
        Vector3 targetDirection = targetLocation - poleTransform.position;
        float angel = Vector3.SignedAngle(targetDirection, poleTransform.forward, Vector3.up);

        if (angel >= -1 && angel <= 1)
        {
            rotationDirection = 0;
        }
        else if (angel < 0)
        {
            rotationDirection = 1;
        }
        else if (angel > 0)
        {
            rotationDirection = -1;
        }

        if (rotationDirection == -1 && currentRotation <= minRotationClamp || rotationDirection == 1 && currentRotation >= maxRotationClamp)
        {
            rotationDirection = 0;
        }

        RotateTurret();
    }

    private void RotateTurret()
    {

        currentRotation += (rotationSpeed * GameManager.Instance.ObjectsGameSpeed) * rotationDirection * Time.deltaTime;
        poleTransform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private void ChargingAndAttack()
    {
        if (currentShootTimer >= shootTimer)
        {
            targetHealth.TakeDamage(new(damageStruct.DamageAmount, transform));
            currentShootTimer = 0;
        }
        else
        {
            currentShootTimer += Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;
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
                    lastPlayerLocation = playerTransform.position;
                    currentTimerToLoseTarget = 0;
                    canReset = true;
                    playerTransform = null;
                }
                break;
        }
    }


}
