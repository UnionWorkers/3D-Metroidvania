using UnityEngine;
using Managers;
using Entities;
using Utils.Triggers;
using System.Collections.Generic;
using Utils.Effect;



public class SpinningFan : BaseEntity
{
    enum GameSpeedOpen
    {
        NormalGameSpeed,
        SlowedGameSpeed,
    }

    [SerializeField] private RotatingUtil rotatingUtil;

    [SerializeField] private GameSpeedOpen whenIsPassableGameSpeed = GameSpeedOpen.NormalGameSpeed;
    [SerializeField] private List<TriggerCollisionMessenger> triggerCollisionMessengers = new();
    [SerializeField] private GameObject wallBlocker;
    private bool wallIsBlocking = true;
    private float currentGameSpeed = 1f;

    public override void OnInitialize()
    {
        if (wallBlocker == null)
        {
            Debug.LogError("Has no wall blocker, fan will not work");
            EntityState = EntityState.Disabled;
            return;
        }

        if (triggerCollisionMessengers.Count <= 0)
        {
            Debug.LogError("triggerCollisionMessengers is empty, fan will not work");
            EntityState = EntityState.Disabled;
        }

        foreach (TriggerCollisionMessenger item in triggerCollisionMessengers)
        {
            if (item == null)
            {
                Debug.LogWarning("there is a null in triggerCollisionMessengers, fan will not work");
                EntityState = EntityState.Disabled;
                return;
            }

            item.OnTriggerCollision += TriggerMessage;
        }
    }

    private void TriggerMessage(Collider collider, CollisionTriggerType type)
    {
        switch (type)
        {
            case CollisionTriggerType.Enter:
                if (collider.CompareTag("Player"))
                {
                    IHealth health = collider.GetComponent<IHealth>();

                    if (health != null)
                    {
                        health.TakeDamage(new(1, transform));
                    }
                }

                break;
        }
    }

    public override void OnFixedUpdate(float gameSpeed)
    {
        currentGameSpeed = gameSpeed;

        switch (whenIsPassableGameSpeed)
        {
            case GameSpeedOpen.NormalGameSpeed:
                CheckIfWallCanOpen(1.01f);
                break;
            case GameSpeedOpen.SlowedGameSpeed:
                CheckIfWallCanOpen(0.98f);
                break;
        }

        // add ro
        rotatingUtil.RotateObject(transform, Time.fixedDeltaTime);
    }

    private void CheckIfWallCanOpen(float gameSpeedRequired)
    {
        if (currentGameSpeed <= gameSpeedRequired && wallIsBlocking)
        {
            wallBlocker.SetActive(false);
            wallIsBlocking = false;
        }
        else if (currentGameSpeed > gameSpeedRequired && !wallIsBlocking)
        {
            wallBlocker.SetActive(true);
            wallIsBlocking = true;
        }
    }
}
