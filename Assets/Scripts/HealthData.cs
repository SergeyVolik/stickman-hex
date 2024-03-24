using System;
using Unity.Mathematics;
using UnityEngine;

namespace Prototype
{
    public class HealthChangeData
    {
        public int PrevValue;
        public int CurrentValue;
        public GameObject Source;
        public bool IsDamage => CurrentValue < PrevValue;
    }

    public interface IResurrectable
    {
        public event Action onResurrected;
        public bool IsDead { get; }
        public void Resurrect();
    }

    public interface IKillable
    {
        public event Action onDeath;
        public void Kill();
        public bool IsDead { get; }
    }

    public interface IHealthData
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
    }

    public interface IDamageable
    {
        public void DoDamage(int damage, GameObject source);
    }

    public interface IHealable
    {
        public void DoHeal(int heal, GameObject source);
    }

    public class HealthData : MonoBehaviour, IResurrectable, IKillable, IDamageable, IHealable
    {
        public int maxHealth = 10;   
        public int currentHealth = 10;

        public event Action<HealthChangeData> onHealthChanged = delegate { };
        public event Action onDeath = delegate { };
        public event Action onResurrected = delegate { };

        public bool IsDead => currentHealth == 0;

        public bool HasMaxHealth()
        {
            return currentHealth == maxHealth;
        }

        public void Resurrect()
        {
            DoHeal(maxHealth, null);
            onResurrected.Invoke();
        }

        public void DoHeal(int heal, GameObject source)
        {
            ChangeHealth(heal, source);
        }

        private void ChangeHealth(int value, GameObject source)
        {
            var prev = currentHealth;
            currentHealth = currentHealth + value;

            currentHealth = math.clamp(currentHealth, 0, maxHealth);

            if (currentHealth != prev)
            {
                onHealthChanged.Invoke(new HealthChangeData
                {
                    CurrentValue = currentHealth,
                    PrevValue = prev,
                    Source = source
                });
            }

            if (currentHealth == 0 && prev != 0)
            {
                onDeath.Invoke();
            }
        }

        public void DoDamage(int damage, GameObject source)
        {
            ChangeHealth(-damage, source);
        }

        public void Kill()
        {
            ChangeHealth(0, null);
        }
    }
}
