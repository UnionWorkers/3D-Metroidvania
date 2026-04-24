using System;
using Entities;
using Managers;
using UnityEngine;
using UnityEngine.Splines;

public class SpickGuyController : BaseEntity, IHealth
{
    [SerializeField] private DamageStruct damageStruct;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private SplineAnimate splineAnimate = null;
    [SerializeField] private float maxSpeed = 4f;
    private float previousGameSpeed = 1f;
    private float previousNormalizedSpeed = 1f;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            IHealth playerHealth = collision.GetComponent<IHealth>();
            if (playerHealth.GetHealth <= 0)
            {
                return;
            }
            playerHealth.TakeDamage(new(damageStruct.DamageAmount, transform));
        }
    }

    public void Heal(int inHealth) { return; }

    public void TakeDamage(DamageInfo inDamageInfo)
    {
        healthComponent.CurrentHealth -= inDamageInfo.DamageAmount;
        OnHealthChanged?.Invoke(healthComponent.CurrentHealth);
        if (healthComponent.CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
            EntityState = EntityState.ToDestroy;
            splineAnimate.enabled = false;
        }
    }

    public override void OnInitialize()
    {
        playerTransform = GameManager.Instance.PlayerController.GetTransform;
        splineAnimate = GetComponent<SplineAnimate>();

        previousGameSpeed = GameManager.Instance.ObjectsGameSpeed;
        previousNormalizedSpeed = splineAnimate.NormalizedTime;
        splineAnimate.MaxSpeed = maxSpeed * previousGameSpeed;

        GameManager.Instance.OnGameStateChanged += GameStateChanged;

    }

    private void GameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Running:
                if (!splineAnimate.IsPlaying)
                {
                    splineAnimate.Play();
                }
                break;
            case GameState.Paused:
                if (splineAnimate.IsPlaying)
                {
                    splineAnimate.Pause();
                }
                break;
        }
    }

    public override void OnUpdate()
    {
        if (previousGameSpeed != GameManager.Instance.ObjectsGameSpeed)
        {
            previousNormalizedSpeed = splineAnimate.NormalizedTime;
            previousGameSpeed = GameManager.Instance.ObjectsGameSpeed;
            splineAnimate.MaxSpeed = maxSpeed * previousGameSpeed;
            splineAnimate.NormalizedTime = previousNormalizedSpeed;
        }

        transform.forward = playerTransform.position - transform.position;
    }
}
