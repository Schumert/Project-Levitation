using System;
using UnityEngine;


[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour, IDamageable
{

    public float CurrentHealth { get; private set; }

    [SerializeField]
    private float MaxHealth = 100f;  // Inspector’da görünür
    public float maxHealth => MaxHealth;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        OnDamaged?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth);
        if (CurrentHealth <= 0f)
            OnDied?.Invoke();

        Debug.Log("HASAR ALINDI. ŞUANKİ HP: " + CurrentHealth);
    }

    public event Action<float> OnHealthChanged;

    public event Action<float> OnDamaged;
    public event Action OnDied;
}
