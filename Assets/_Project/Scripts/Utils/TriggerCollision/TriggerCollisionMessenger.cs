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
        private void OnTriggerEnter(Collider other)
        {
            OnTriggerCollision?.Invoke(other, CollisionTriggerType.Enter);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerCollision?.Invoke(other, CollisionTriggerType.Exit);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTriggerCollision?.Invoke(other, CollisionTriggerType.Stay);
        }
    }

}