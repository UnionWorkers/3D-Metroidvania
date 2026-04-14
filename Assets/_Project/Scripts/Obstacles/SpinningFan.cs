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
    
    [SerializeField] private GameSpeedOpen WhenIsPassableGameSpeed = GameSpeedOpen.NormalGameSpeed;
    [SerializeField]
    private List<TriggerCollisionMessenger> triggerCollisionMessengers = new();
    [SerializeField] private GameObject wallBlocker;
    private bool wallIsBlocking = true;
    private float gameSpeed = 1f;

    public override void OnInitialize()
    {
        foreach (TriggerCollisionMessenger item in triggerCollisionMessengers)
        {
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

    public override void OnFixedUpdate()
    {
        gameSpeed = GameManager.Instance.ObjectsGameSpeed;

        switch (WhenIsPassableGameSpeed)
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
        if (gameSpeed <= gameSpeedRequired && wallIsBlocking)
        {
            wallBlocker.SetActive(false);
            wallIsBlocking = false;
        }
        else if (gameSpeed > gameSpeedRequired && !wallIsBlocking)
        {
            Debug.Log(wallBlocker);

            wallBlocker.SetActive(true);
            wallIsBlocking = true;
        }
    }
}
