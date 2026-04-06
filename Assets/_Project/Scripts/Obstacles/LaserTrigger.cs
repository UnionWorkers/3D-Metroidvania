using UnityEngine;
using Utils.Triggers;

public class LaserTrigger : TriggerCollisionMessenger
{

    public System.Action<Collider, CollisionTriggerType, Transform> OnLaserTriggerCollision;

    protected override void OnTriggerEnter(Collider other)
    {
        OnLaserTriggerCollision?.Invoke(other, CollisionTriggerType.Enter, transform);
    }


}
