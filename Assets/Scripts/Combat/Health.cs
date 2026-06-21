using System;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    public class Health : MonoBehaviour
    {
        int maxHealth;
        int currentHealth;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;

        public event Action<int, int> OnHealthChanged;
        public event Action<int, int, int> OnDamaged;
        public event Action OnDeath;

        public void Initialize(int health)
        {
            maxHealth = health;
            currentHealth = health;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive)
                return;

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamaged?.Invoke(amount, currentHealth, maxHealth);

            if (currentHealth <= 0)
                OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
