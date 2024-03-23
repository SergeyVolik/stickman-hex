using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Prototype
{
    [RequireComponent(typeof(HealthData))]
    public class FarmableObject : MonoBehaviour
    {
        [SerializeField]
        private WeaponType m_RequiredWeapon;

        public WeaponType RequiredWeapon => m_RequiredWeapon;

        public GameObject[] Parts;

        [SerializeField]
        private ResourceContainer m_StartResource;
        private ResourceContainer m_CurrentResource;

        private HealthData m_Health;
        private Transform m_Transform;

        private void Awake()
        {
            m_Health = GetComponent<HealthData>();

            m_CurrentResource = m_StartResource.DeepClone();

            m_Health.onHealthChanged += M_Health_onHealthChaged;
            m_Health.onDeath += M_Health_onDeath;
            m_Transform = transform;
        }

        private void M_Health_onDeath()
        {
            
        }

        private void M_Health_onHealthChaged(HealthChangeData obj)
        {
            var damagedPercent = m_Health.CurrentHealth / (float)m_Health.maxHealth;

            int toDeactivate = (int)(damagedPercent * Parts.Length);

            for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].SetActive(toDeactivate > i || i == 0 && !m_Health.HasMaxHealth());
            }

            m_Transform.DOShakeScale(0.1f);
        }
    }
}
