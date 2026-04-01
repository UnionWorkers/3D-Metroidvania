using System;
using UnityEngine;

namespace Utils.Triggers
{
    public class HealthTriggerCollision : TriggerCollisionMessenger, IHealth
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        [SerializeField] private HealthComponent healthComponent;

        private void Awake()
        {
            healthComponent.Initialize();
        }

        public void Heal(int inHealth) { return; }

        public void TakeDamage(DamageInfo inDamageInfo)
        {
            healthComponent.CurrentHealth -= inDamageInfo.DamageAmount;
            OnHealthChanged?.Invoke(healthComponent.CurrentHealth);
            if (healthComponent.CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }

}