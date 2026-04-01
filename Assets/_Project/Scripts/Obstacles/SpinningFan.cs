using UnityEngine;
using Managers;
using Entities;
using Utils.Triggers;
using System.Collections.Generic;



public class SpinningFan : BaseEntity
{
    enum GameSpeedOpen
    {
        NormalGameSpeed,
        SlowedGameSpeed,
    }

    [SerializeField] private Vector3 Rotation;
    [SerializeField] private GameSpeedOpen WhenIsPassableGameSpeed = GameSpeedOpen.NormalGameSpeed;
    public float Speed;
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

    public override void OnUpdate()
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

        transform.Rotate(Rotation * Speed * Time.deltaTime * gameSpeed);
    }

    private void CheckIfWallCanOpen(float gameSpeedRequired)
    {
        if (gameSpeed <= gameSpeedRequired && wallIsBlocking)
        {

            Debug.Log(wallBlocker);
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
