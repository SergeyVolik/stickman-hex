using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.WSA;
using Zenject;
using Zenject.ReflectionBaking.Mono.Cecil;

namespace Prototype
{
    public class ResourceRecycling : MonoBehaviour
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

        [SerializeField]
        private RecycleUI m_UIPrefab;

        private RecycleUI m_UIInstance;
        private WordlToScreenItem m_WorldToScreenHandle;
        private ActivateableByDistance m_ActByDistHandle;
        [SerializeField]
        private Transform m_UiBindPoint;

        public float distanceToActivateUI = 2f;

        [Inject]
        public void Construct(
            PlayerResources resources,
            IPlayerFactory playerFactory,
            WorldToScreenUIManager wtsManager,
            ActivateByDistanceToPlayerManager actManager,
            TransferMoveManager transManager)
        {
            m_PlayerResources = resources;
            m_PlayerFactory = playerFactory;
            m_wtsManager = wtsManager;
            m_actManager = actManager;
            m_TransManager = transManager;
        }

        private void Awake()
        {
            m_UIInstance = GameObject.Instantiate(m_UIPrefab, m_wtsManager.Root);
        }

        private void OnEnable()
        {
            m_WorldToScreenHandle = m_wtsManager.Register(new WordlToScreenItem
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

            m_UIInstance.m_TransferButton.onClick.AddListener(() => {

                int nextRecicle = (int)(m_UIInstance.m_Slider.value);


                m_PlayerResources.resources.RemoveResource(sourceResource, nextRecicle * itemsToDestResource);
                m_PlayerResources.resources.AddResource(destinationResource, nextRecicle);

                int transferVisualCount = 5;
                var spawnPos = m_PlayerFactory.CurrentPlayerUnit.transform.position;
                const float pushForce = 7f;

                for (int i = 0; i < transferVisualCount; i++)
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

            m_UIInstance.m_Slider.onValueChanged.AddListener((v) => {
                Resources_onResourceChanged(null, 0);
            });

            m_PlayerResources.resources.onResourceChanged += Resources_onResourceChanged;
            
            Resources_onResourceChanged(null, 0);
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

        private void OnDisable()
        {
            m_wtsManager.Unregister(m_WorldToScreenHandle);
            m_actManager.Unregister(m_ActByDistHandle);
            m_PlayerResources.resources.onResourceChanged -= Resources_onResourceChanged;
        }
    }
}