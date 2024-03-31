using System;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Prototype
{
    [System.Serializable]
    public class ResourceRecyclingSave
    {
        public int itemToRecycle;
    }

    public class ResourceRecycling : MonoBehaviour, ISaveable<ResourceRecyclingSave>
    {
        public ResourceTypeSO sourceResource;
        public ResourceTypeSO destinationResource;

        [Min(1)]
        public int itemsToDestResource = 1;

        private PlayerResources m_PlayerResources;
        private IPlayerFactory m_PlayerFactory;

        private WorldToScreenUIManager m_wtsManager;
        private ActivateByDistanceToPlayerManager m_actManager;
        private TransferMoveManager m_TransManager;
        private WorldSpaceMessageFactory m_wpsFactory;

        [SerializeField]
        private RecycleUI m_UIPrefab;
        private RecycleUI m_UIInstance;

        [SerializeField]
        private RecycleProcessViewUI m_UIRecycleViewUiPrefab;
        private RecycleProcessViewUI m_UIRecycleViewInstance;

        private WordlToScreenUIItem m_WorldToScreenHandle;
        private ActivateableByDistance m_ActByDistHandle;
        [SerializeField]
        private Transform m_UiBindPoint;

        [SerializeField]
        private Transform m_WorldMessageSpawnPoint;

        public float distanceToActivateUI = 2f;

        [Min(0.01f)]
        public float recycleItemDuration = 1f;
        public int itemToRecycle;
        public float recycleT;
        private WordlToScreenUIItem m_WorldToScreenHandle2;

        public event Action onProcessUpdatedChanged = delegate { };

        [Inject]
        public void Construct(
             PlayerResources resources,
             IPlayerFactory playerFactory,
             WorldToScreenUIManager wtsManager,
             ActivateByDistanceToPlayerManager actManager,
             TransferMoveManager transManager,
             WorldSpaceMessageFactory wpsFactory)
        {
            m_PlayerResources = resources;
            m_PlayerFactory = playerFactory;
            m_wtsManager = wtsManager;
            m_actManager = actManager;
            m_TransManager = transManager;
            m_wpsFactory = wpsFactory;
        }

        public float GetCurrentProcessProgress() => recycleT / recycleItemDuration;

        private void Awake()
        {
            m_UIRecycleViewInstance = GameObject.Instantiate(m_UIRecycleViewUiPrefab, m_wtsManager.Root);
            m_UIInstance = GameObject.Instantiate(m_UIPrefab, m_wtsManager.Root);

            m_UIInstance.Deactivate();
        }


        private void Update()
        {
            if (itemToRecycle == 0)
                return;

            recycleT += Time.deltaTime;

            if (recycleT > recycleItemDuration)
            {
                recycleT = 0;
                itemToRecycle--;

                m_wpsFactory.SpawnAtPosition(m_WorldMessageSpawnPoint.position, "+1", destinationResource.resourceIcon);

                m_PlayerResources.resources.AddResource(destinationResource, 1);
            }

            onProcessUpdatedChanged.Invoke();
        }

        private void OnEnable()
        {
            m_UIRecycleViewInstance.Bind(this);

            m_WorldToScreenHandle2 = m_wtsManager.Register(new WordlToScreenUIItem
            {
                worldPositionTransform = m_UiBindPoint,
                item = m_UIRecycleViewInstance.GetComponent<RectTransform>()
            });

            m_WorldToScreenHandle = m_wtsManager.Register(new WordlToScreenUIItem
            {
                worldPositionTransform = m_UiBindPoint,
                item = m_UIInstance.GetComponent<RectTransform>()
            });

            m_ActByDistHandle = m_actManager.Register(new ActivateableByDistance
            {
                DistanceObj = m_UiBindPoint,
                DistanceToActivate = distanceToActivateUI,
                ItemToActivate = m_UIInstance
            });

            m_UIInstance.m_TransferButton.onClick.AddListener(() =>
            {
                int nextRecicle = (int)(m_UIInstance.m_Slider.value);

                itemToRecycle += nextRecicle;
                m_PlayerResources.resources.RemoveResource(sourceResource, nextRecicle * itemsToDestResource);

                int maxTransferVisual = itemsToDestResource * 2;
                const float pushForce = 7f;

                int transferVisual = Mathf.Min(nextRecicle * itemsToDestResource, maxTransferVisual);

                var spawnPos = m_PlayerFactory.CurrentPlayerUnit.transform.position;

                for (int i = 0; i < transferVisual; i++)
                {
                    var resourceObjectInstance = GameObjectPool.GetPoolObject(sourceResource.Resource3dItem);

                    resourceObjectInstance.transform.position = spawnPos;
                    resourceObjectInstance.SetActive(true);

                    var rb = resourceObjectInstance.GetComponent<Rigidbody>();
                    var initialVelocity = Vector3.up * pushForce;

                    m_TransManager.Transfer3dObject(rb, spawnPos, initialVelocity, transform, moveDuration: 0.5f,
                        onComplete: () =>
                        {
                            rb.GetComponent<PoolObject>().Release();
                        });
                }
            });

            m_UIInstance.m_Slider.onValueChanged.AddListener((v) =>
            {
                Resources_onResourceChanged(null, 0);
            });

            m_PlayerResources.resources.onResourceChanged += Resources_onResourceChanged;
            Resources_onResourceChanged(null, 0);
        }

        private void OnDisable()
        {
            m_wtsManager.Unregister(m_WorldToScreenHandle2);
            m_wtsManager.Unregister(m_WorldToScreenHandle);
            m_actManager.Unregister(m_ActByDistHandle);
            m_PlayerResources.resources.onResourceChanged -= Resources_onResourceChanged;
        }

        private void Resources_onResourceChanged(ResourceTypeSO arg1, int arg2)
        {
            var playerResourceNumber = m_PlayerResources.resources.GetResource(sourceResource);

            m_UIInstance.m_TransferButton.enabled = playerResourceNumber >= itemsToDestResource;

            int canBeRecicled = playerResourceNumber / itemsToDestResource;

            m_UIInstance.m_Slider.minValue = 0;
            m_UIInstance.m_Slider.maxValue = canBeRecicled;

            int nextRecicle = (int)(m_UIInstance.m_Slider.value);

            m_UIInstance.m_SliderView.minValue = 0;
            m_UIInstance.m_SliderView.maxValue = 1f;
            float offsetteValue = math.remap(0, 1f, 0, 1 - m_UIInstance.m_ViewInitValue, canBeRecicled != 0 ? nextRecicle / (float)canBeRecicled : 0f);
            m_UIInstance.m_SliderView.value = m_UIInstance.m_ViewInitValue + offsetteValue;

            m_UIInstance.sourceResourceUI.SetSprite(sourceResource.resourceIcon);
            m_UIInstance.sourceResourceUI.SetText(TextUtils.IntToText(nextRecicle * itemsToDestResource));

            m_UIInstance.destionationResourceUI.SetSprite(destinationResource.resourceIcon);
            m_UIInstance.destionationResourceUI.SetText(TextUtils.IntToText(nextRecicle));
        }

        public ResourceRecyclingSave Save()
        {
            return new ResourceRecyclingSave { 
                 itemToRecycle = itemToRecycle
            };
        }

        public void Load(ResourceRecyclingSave data)
        {
            if (data == null)
                return;

            itemToRecycle = data.itemToRecycle;
        }
    }
}