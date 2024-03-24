using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Prototype
{
    [RequireComponent(typeof(HealthData))]
    public class FarmableObject : MonoBehaviour
    {
        [SerializeField]
        private WeaponType m_RequiredWeapon;

        public WeaponType RequiredWeapon => m_RequiredWeapon;

        public GameObject[] Parts;
        public WorldSpaceMessage MessagePrefab;
        [SerializeField]
        private ResourceContainer m_StartResource;
        private ResourceContainer m_CurrentResource;

        private HealthData m_Health;
        private Collider m_Collider;
        private Transform m_Transform;
        private TransferMoveManager m_TransManager;

        [Inject]
        void Construct(TransferMoveManager transManager)
        {
            m_TransManager = transManager;
        }

        private void Awake()
        {
            m_Health = GetComponent<HealthData>();
            m_Collider = GetComponent<Collider>();
            m_CurrentResource = m_StartResource.DeepClone();

            m_Health.onHealthChanged += M_Health_onHealthChaged;
            m_Health.onDeath += M_Health_onDeath;
            m_Health.onResurrected += M_Health_onResurrected;
            m_Transform = transform;
        }

        private void M_Health_onResurrected()
        {
            m_Transform.localScale = new Vector3(1, 0, 1);
            m_Transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
        }

        private void M_Health_onDeath()
        {

        }

        private void M_Health_onHealthChaged(HealthChangeData obj)
        {
            var currentHealthPercent = m_Health.currentHealth / (float)m_Health.maxHealth;
            UpdateObjectParts();

            m_Collider.enabled = !m_Health.IsDead;

            //m_Transform.DOShakeScale(0.1f, new Vector3(0, 1, 0));
            m_Transform.DOPunchScale(new Vector3(0, -0.3f, 0), 0.15f);
            if (obj.Source != null && obj.IsDamage)
            {
                var holderObj = obj.Source;

                if (!TryAddResource(holderObj) && holderObj.TryGetComponent<IOwnable>(out var owner))
                {
                    TryAddResource(owner.Owner);
                }
            }
        }

        private void UpdateObjectParts()
        {
            if (m_Health.HasMaxHealth())
            {
                for (int i = 0; i < Parts.Length; i++)
                {
                    Parts[i].SetActive(true);
                }
            }
            else if (m_Health.IsDead)
            {
                for (int i = 0; i < Parts.Length; i++)
                {
                    Parts[i].SetActive(false);
                }
            }
            else
            {
                var currentHealthPercent = m_Health.currentHealth / (float)m_Health.maxHealth;
                var lastIndex = Parts.Length - 1;
                int ActiveParts = lastIndex - (int)(currentHealthPercent * lastIndex);

                for (int i = 0; i < Parts.Length; i++)
                {
                    bool activateFirstElement = i == 0 && m_Health.HasMaxHealth();
                    bool activateLastElement = !m_Health.IsDead && i == Parts.Length - 1;
                    bool otherPartsActivate = ActiveParts <= i;

                    Parts[i].SetActive(otherPartsActivate || activateFirstElement || activateLastElement);
                }
            }
        }

        private bool TryAddResource(GameObject holderObj)
        {
            float itemSpeed = 7f;

            if (holderObj.TryGetComponent<IResourceHolder>(out var holder))
            {
                foreach (var item in m_StartResource.ResourceIterator())
                {
                    var resourceType = item.Key;
                    holder.Resources.AddResource(resourceType, item.Value);
                    var instance = GameObjectPool.GetPoolObject(MessagePrefab);
                    instance.Show(m_Transform.position, $"+{item.Value}", item.Key.resourceIcon);

                    for (int i = 0; i < 3; i++)
                    {
                        var resourceObjectInstance = GameObjectPool.GetPoolObject(resourceType.Resource3dItem);
                        resourceObjectInstance.transform.position = m_Transform.position;
                        resourceObjectInstance.SetActive(true);

                        var rb = resourceObjectInstance.GetComponent<Rigidbody>();

                        var initialVelocity = Vector3.up * itemSpeed;

                        m_TransManager.Transfer3dObject(rb, m_Transform.position, initialVelocity, holder.CenterPoint, onComplete: () =>
                        {
                            rb.GetComponent<PoolObject>().Release();
                        });
                    }
                }

                return true;
            }

            return false;
        }
    }
}
