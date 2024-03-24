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

    public class HealthData : MonoBehaviour
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
    }
}
