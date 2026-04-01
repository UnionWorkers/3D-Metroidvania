using System;
using UnityEngine;

public struct DamageInfo
{
    public int DamageAmount;
    public Transform HitObject;

    public DamageInfo(int inDamageAmount = 0, Transform inHitObject = null)
    {
        DamageAmount = inDamageAmount;
        HitObject = inHitObject;
    }
}

public interface IHealth
{
    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    public int GetHealth => throw new NotImplementedException();
    public void Heal(int inHealth);
    public void TakeDamage(DamageInfo inDamageInfo);
}

[System.Serializable]
public class HealthComponent
{
    [UnityEngine.SerializeField] private int maxHealth = 3;
    [NonSerialized] public int CurrentHealth;
    public bool IsAlive => CurrentHealth > 0;
    public int MaxHealth => maxHealth;

    public HealthComponent()
    {
        CurrentHealth = maxHealth;
    }

    public void Initialize()
    {
        CurrentHealth = maxHealth;
    }
}
