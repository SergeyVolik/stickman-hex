using System;
using Unity.Mathematics;
using UnityEngine;

namespace Prototype
{
    public struct HealthChangeData
    {
        public int PrevValue;
        public int CurrentValue;
    }

    public class HealthData : MonoBehaviour
    {
        public int maxHealth = 10;

        [SerializeField]
        private int m_CurrentHealth = 10;

        public int CurrentHealth
        {
            get { return m_CurrentHealth; }
            set
            {
                var prev = m_CurrentHealth;
                m_CurrentHealth = math.clamp(value, 0, maxHealth);

                if (m_CurrentHealth != prev)
                {
                    onHealthChanged.Invoke(new HealthChangeData
                    {
                        CurrentValue = m_CurrentHealth,
                        PrevValue = prev
                    });
                }

                if (m_CurrentHealth == 0 && prev != 0)
                {
                    onDeath.Invoke();
                }
            }
        }
        public event Action<HealthChangeData> onHealthChanged = delegate { };
        public event Action onDeath = delegate { };
        public bool IsDead => m_CurrentHealth == 0;

        public bool HasMaxHealth()
        {
            return m_CurrentHealth == maxHealth;
        }
        public void AddHealth(int toAdd)
        {
            CurrentHealth = m_CurrentHealth + toAdd;
        }

        public void RemoveHealth(int toRemove)
        {
            CurrentHealth = m_CurrentHealth - toRemove;
        }
    }
}
