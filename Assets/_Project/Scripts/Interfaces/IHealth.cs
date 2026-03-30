using System;

public interface IHealth
{
    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    public int GetHealth => throw new NotImplementedException();
    public void Heal(int inHealth);
    public void TakeDamage(int inDamage);
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
