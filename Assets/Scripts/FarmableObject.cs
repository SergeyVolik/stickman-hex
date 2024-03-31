using DG.Tweening;
using UnityEngine;
using Zenject;
using CandyCoded.HapticFeedback;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Prototype
{
    [RequireComponent(typeof(HealthData))]
    public class FarmableObject : MonoBehaviour
    {
        [SerializeField]
        private WeaponType m_RequiredWeapon;

        [SerializeField]
        private ParticleSystem m_HitParticle;

        [SerializeField]
        private ParticleSystem m_DeathParticle;

        [SerializeField]
        private ParticleSystem m_PartRemoveParticle;

        public WeaponType RequiredWeapon => m_RequiredWeapon;

        [SerializeField]
        private Transform m_PartsParent;

        [FormerlySerializedAs("Parts")]
        [SerializeField]
        private GameObject[] m_Parts;

        [SerializeField]
        private ResourceContainer m_StartResource;
        private ResourceContainer m_ArlreadyDroppedResources;

        private HealthData m_Health;
        private Collider m_Collider;
        private TransferMoveManager m_TransManager;
        private WorldSpaceMessageFactory m_wsmFactory;
        int m_PrevActivateParts;    

        [Inject]
        void Construct(TransferMoveManager transManager, WorldSpaceMessageFactory wsmFactory)
        {
            m_TransManager = transManager;
            m_wsmFactory = wsmFactory;
        }

       
        private void Awake()
        {
            m_Health = GetComponent<HealthData>();
            m_Collider = GetComponent<Collider>();
            m_ArlreadyDroppedResources = new ResourceContainer();

            m_PrevActivateParts = m_Parts.Length;
            m_Health.onHealthChanged += M_Health_onHealthChaged;
            m_Health.onDeath += M_Health_onDeath;
            m_Health.onResurrected += M_Health_onResurrected;
        }

        private void M_Health_onResurrected()
        {
            m_PartsParent.localScale = new Vector3(1, 0, 1);
            m_PartsParent.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
            m_ArlreadyDroppedResources.Clear();
        }

        private void M_Health_onDeath()
        {
            if (m_DeathParticle != null)
            {
                m_DeathParticle.Play();
            }
        }

        private void M_Health_onHealthChaged(HealthChangeData obj)
        {
            var currentHealthPercent = m_Health.currentHealth / (float)m_Health.maxHealth;
            UpdateObjectParts();

            m_Collider.enabled = !m_Health.IsDead;
            m_PartsParent.DOPunchScale(new Vector3(0, -0.3f, 0), 0.15f);

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
                for (int i = 0; i < m_Parts.Length; i++)
                {
                    m_Parts[i].SetActive(true);
                }
            }
            else if (m_Health.IsDead)
            {
                for (int i = 0; i < m_Parts.Length; i++)
                {
                    m_Parts[i].SetActive(false);
                }
            }
            else
            {
                var currentHealthPercent = m_Health.currentHealth / (float)m_Health.maxHealth;
                var lastIndex = m_Parts.Length - 1;
                int ActiveParts = lastIndex - (int)(currentHealthPercent * lastIndex);

               
                bool partRemoved = false;

                for (int i = 0; i < m_Parts.Length; i++)
                {
                    bool activateFirstElement = i == 0 && m_Health.HasMaxHealth();
                    bool activateLastElement = !m_Health.IsDead && i == m_Parts.Length - 1;
                    bool otherPartsActivate = ActiveParts <= i;

                    bool activatePart = otherPartsActivate || activateFirstElement || activateLastElement;
                    if (partRemoved == false)
                    {
                        partRemoved = m_Parts[i].activeSelf && !activatePart;
                    }
                    m_Parts[i].SetActive(otherPartsActivate || activateFirstElement || activateLastElement);
                }

                if (partRemoved == true)
                {
                    if (m_PartRemoveParticle != null)
                    {
                        m_PartRemoveParticle.Play();
                    }
                }


                m_PrevActivateParts = ActiveParts;
            }
        }

        private bool TryAddResource(GameObject holderObj)
        {
            if (holderObj.TryGetComponent<IResourceHolder>(out var holder))
            {
                var currentHealthPercent = m_Health.currentHealth / (float)m_Health.maxHealth;

                const float itemSpeed = 7f;

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
                               
                    m_wsmFactory.SpawnAtPosition(m_PartsParent.position, $"+{toDrop}", item.Key.resourceIcon);

                    int transferVisualCount = math.clamp(toDrop, 0, maxFropItems);

                    for (int i = 0; i < transferVisualCount; i++)
                    {
                        var resourceObjectInstance = GameObjectPool.GetPoolObject(resourceType.Resource3dItem);
                        resourceObjectInstance.transform.position = m_PartsParent.position;
                        resourceObjectInstance.SetActive(true);

                        var rb = resourceObjectInstance.GetComponent<Rigidbody>();

                        var initialVelocity = Vector3.up * itemSpeed;

                        m_TransManager.Transfer3dObject(rb, m_PartsParent.position, initialVelocity, holder.CenterPoint, moveDuration: 0.5f,
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
