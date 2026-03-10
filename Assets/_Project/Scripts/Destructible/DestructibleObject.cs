using System;
using UnityEngine;

public class DestructibleObject : MonoBehaviour, IHealth
{
    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    [SerializeField] private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent.Initialize();
    }

    // This object cant be healed 
    public void Heal(int inHealth) { }

    // Takes set amount of damage, irrelevant of inDamage  
    public void TakeDamage(int inDamage)
    {
        healthComponent.CurrentHealth -= 1;

        if (!healthComponent.IsAlive)
        {
            healthComponent.CurrentHealth = 0;
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}
