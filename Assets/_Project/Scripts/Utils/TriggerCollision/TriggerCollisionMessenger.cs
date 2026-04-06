using System;
using UnityEngine;

namespace Utils.Triggers
{
    public enum CollisionTriggerType : byte
    {
        Enter,
        Exit,
        Stay
    }

    public class TriggerCollisionMessenger : MonoBehaviour
    {
        public Action<Collider, CollisionTriggerType> OnTriggerCollision;
        protected virtual void OnTriggerEnter(Collider other)
        {
            OnTriggerCollision?.Invoke(other, CollisionTriggerType.Enter);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            OnTriggerCollision?.Invoke(other, CollisionTriggerType.Exit);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            OnTriggerCollision?.Invoke(other, CollisionTriggerType.Stay);
        }
    }

}