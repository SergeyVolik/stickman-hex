using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using CandyCoded.HapticFeedback;
using Unity.Mathematics;

namespace Prototype
{
    [RequireComponent(typeof(HealthData))]
    public class FarmableObject : MonoBehaviour
    {
        [SerializeField]
        private WeaponType m_RequiredWeapon;

        [SerializeField]
        private ParticleSystem m_HitParticle;

        public WeaponType RequiredWeapon => m_RequiredWeapon;

        public GameObject[] Parts;
        public WorldSpaceMessage MessagePrefab;
        [SerializeField]
        private ResourceContainer m_StartResource;
        private ResourceContainer m_ArlreadyDroppedResources;

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
            m_ArlreadyDroppedResources = new ResourceContainer();

            m_Health.onHealthChanged += M_Health_onHealthChaged;
            m_Health.onDeath += M_Health_onDeath;
            m_Health.onResurrected += M_Health_onResurrected;
            m_Transform = transform;
        }

        private void M_Health_onResurrected()
        {
            m_Transform.localScale = new Vector3(1, 0, 1);
            m_Transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
            m_ArlreadyDroppedResources.Clear();
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
                if (m_HitParticle != null)
                {
                    m_HitParticle.Play();
                }

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
            if (holderObj.TryGetComponent<IResourceHolder>(out var holder))
            {
                var currentHealthPercent = m_Health.currentHealth / (float)m_Health.maxHealth;

                float itemSpeed = 7f;

                const int maxFropItems = 10;

                foreach (var item in m_StartResource.ResourceIterator())
                {
                    ResourceTypeSO resourceType = item.Key;
                    int resourceCount = item.Value;

                    int healthDiffToDrop = resourceCount - Mathf.RoundToInt(resourceCount * currentHealthPercent);

                    var alreadyDropped = m_ArlreadyDroppedResources.GetResource(resourceType);
                    int toDrop = healthDiffToDrop - alreadyDropped;

                    if (toDrop == 0)
                        continue;

                    m_ArlreadyDroppedResources.SetResource(resourceType, healthDiffToDrop);
                    holder.Resources.AddResource(resourceType, toDrop);

                    var worldMessageInst = GameObjectPool.GetPoolObject(MessagePrefab);
                    worldMessageInst.Show(m_Transform.position, $"+{toDrop}", item.Key.resourceIcon);

                    int transferVisualCount = math.clamp(toDrop, 0, maxFropItems);

                    for (int i = 0; i < transferVisualCount; i++)
                    {
                        var resourceObjectInstance = GameObjectPool.GetPoolObject(resourceType.Resource3dItem);
                        resourceObjectInstance.transform.position = m_Transform.position;
                        resourceObjectInstance.SetActive(true);

                        var rb = resourceObjectInstance.GetComponent<Rigidbody>();

                        var initialVelocity = Vector3.up * itemSpeed;

                        m_TransManager.Transfer3dObject(rb, m_Transform.position, initialVelocity, holder.CenterPoint, moveDuration: 0.7f,
                            onComplete: () =>
                        {
                            rb.GetComponent<PoolObject>().Release();
                        });
                    }
                }

                return true;
            }

            if (holderObj.GetComponent<PlayerCharacterInput>())
            {
                HapticFeedback.LightFeedback();
            }

            return false;
        }
    }
}
