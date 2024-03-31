using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Prototype
{
    [System.Serializable]
    public class ZoneTriggerSave
    {
        public List<ResourceSaveItem> currentResources = new List<ResourceSaveItem>();
        public bool finished;
    }

    public class ZoneTrigger : MonoBehaviour, ISaveable<ZoneTriggerSave>
    {
        public class TransferData
        {
            public float toTransfer;
            public int LastTransferedResources;

            public ResourceTypeSO ResourceType;
        }

        [FormerlySerializedAs("ToOpen")]
        public HexLocation[] NextLocations;
        public ResourceContainer ResourceToOpen;
        private ResourceContainer m_CurrentDelayedResources;
        private ResourceContainer m_CurrentRealResources;

        public ZoneTriggerUI ZoneUIPrefab;
        public Transform ZoneUI;

        private PlayerResources m_PlayerRes;
        private TransferMoveManager m_TransManager;
        private WorldToScreenUIManager m_worldToScreen;
        private ActivateByDistanceToPlayerManager m_actByDist;
        private GameResources m_gResources;
        public Transform TransferEndPoint;
        public ParticleSystem TransferParticle;

        public float tickInterval = 0.05f;
        public float ItemMoveDelay = 1f;
        public float itemMoveDuration = 1f;
        public float itemRotAngle = 90;
        public float itemMaxHeight = 4f;
        public float itemVectorAngleOffset = 10;
        public Ease itemMoveEase;
        public Ease resourceTransferEase;
        public int maxTransferTicks = 30;

        private TweenerCore<float, float, FloatOptions>[] m_TransferTween;

        bool m_Inted = false;
        private ZoneTriggerUI m_ZoneUiInstance;
        private WordlToScreenUIItem m_worldToScreenHandle;
        private ActivateableByDistance m_ActivateByDistanceHandle;

        [SerializeField]
        private bool m_Finished;

        [Inject]
        void Construct(
            PlayerResources resources,
            TransferMoveManager transManager,
            WorldToScreenUIManager worldToScreen,
            ActivateByDistanceToPlayerManager actByDist,
            GameResources gResources)
        {
            m_PlayerRes = resources;
            m_TransManager = transManager;
            m_worldToScreen = worldToScreen;
            m_actByDist = actByDist;
            m_gResources = gResources;
        }

        private void Awake()
        {
            Init();

            m_ZoneUiInstance = GameObject.Instantiate(ZoneUIPrefab, m_worldToScreen.Root);
            m_ZoneUiInstance.resourceView.Bind(ResourceToOpen, m_CurrentDelayedResources);

            var activatable = GetComponent<ActivatableObject>();

            activatable.onActivated += EnableUI;
            activatable.onDeactivated += DisableUI;
        }

        private void DisableUI()
        {
            if (m_ZoneUiInstance)
                m_ZoneUiInstance.gameObject.SetActive(false);
        }

        private void EnableUI()
        {
            if (m_ZoneUiInstance)
                m_ZoneUiInstance.gameObject.SetActive(true);
        }

        public void Init()
        {
            if (m_Inted)
                return;

            m_CurrentDelayedResources = new ResourceContainer();
            m_CurrentRealResources = new ResourceContainer();

            m_Inted = true;

            if (NextLocations != null)
            {
                foreach (var item in NextLocations)
                {
                    item.DeactivateInstant();
                }
            }
        }

        private void OnEnable()
        {
            m_worldToScreenHandle = m_worldToScreen.Register(new WordlToScreenUIItem
            {
                item = m_ZoneUiInstance.GetComponent<RectTransform>(),
                worldPositionTransform = ZoneUI
            });

            m_ActivateByDistanceHandle = m_actByDist.Register(new ActivateableByDistance
            {
                DistanceObj = ZoneUI,
                DistanceToActivate = 4,
                ItemToActivate = m_ZoneUiInstance
            });

            EnableUI();
        }

        private void OnDisable()
        {
            m_worldToScreen?.Unregister(m_worldToScreenHandle);
            m_actByDist?.Unregister(m_ActivateByDistanceHandle);

            DisableUI();
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacterInput>();

            if (player)
            {
                var transferFrom = player.GetComponent<IResourceHolder>().CenterPoint;

                List<TransferData> resources = SetupTransferData();

                if (resources.Count == 0)
                {
                    return;
                }

                StartTransfer(transferFrom, resources);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerCharacterInput>())
            {
                StopTransfer();
            }
        }

        private void StopTransfer()
        {
            if (m_TransferTween != null)
            {
                foreach (var item in m_TransferTween)
                {
                    item?.Kill();
                }
            }
        }

        private void CheckFinish()
        {
            if (m_Finished == true)
            {
                return;
            }

            m_Finished = ResourceToOpen.Equals(m_CurrentDelayedResources);

            if (m_Finished == true)
            {
                Ease ease = Ease.Linear;
                var seq = DOTween.Sequence().SetEase(ease);

                float tweenDuration = 0.25f;
                float inserT = 0f;
                seq.Insert(inserT, m_ZoneUiInstance.transform.DOScale(0, tweenDuration));
                inserT += tweenDuration;
                seq.Insert(inserT, transform.DOScale(0, tweenDuration).OnComplete(() => { gameObject.SetActive(false); }));
                inserT += tweenDuration;

                var zonePos = transform.position;
                var items = NextLocations.Select(e => e.transform).OrderBy((e) => Vector3.Distance(zonePos, e.transform.position));

                foreach (var locations in items)
                {
                    DOVirtual.DelayedCall(inserT, locations.GetComponent<HexLocation>().Activate);
                    inserT += tweenDuration;
                }
            }
        }

        private void StartTransfer(Transform transferFrom, List<TransferData> transferList)
        {
            m_TransferTween = new TweenerCore<float, float, FloatOptions>[transferList.Count];

            float tickInterval = this.tickInterval;
            var playerRes = m_PlayerRes.resources;
            int defaultTickNumber = maxTransferTicks;

            //start transfer for each required resource
            for (int i = 0; i < transferList.Count; i++)
            {
                float lastTransferCount = 1;
                float spawnTicksPerTransfer = math.clamp(transferList[i].toTransfer, 0, defaultTickNumber);
                TransferData resourceItem = transferList[i];
                float tweenDestination = transferList[i].toTransfer;
                bool isFinished = false;
                float spawnT = tickInterval;
                bool executeSpawn = lastTransferCount == transferList[i].toTransfer;
                float duration = spawnTicksPerTransfer * tickInterval;

                var trensferTween = DOTween.To(() => lastTransferCount, (newTransferCount) => lastTransferCount = newTransferCount, tweenDestination, duration)
                    .SetEase(resourceTransferEase).OnUpdate(() =>
                    {
                        if (spawnT > tickInterval)
                        {
                            executeSpawn = true;
                            spawnT = 0;
                        }

                        spawnT += Time.deltaTime;

                        if (executeSpawn == false)
                            return;

                        if (isFinished)
                            return;

                        ExecuteSpawn(transferFrom, playerRes, lastTransferCount, resourceItem, tweenDestination, out isFinished);

                        executeSpawn = false;
                    })
                    .OnComplete(() =>
                    {
                        if (isFinished)
                            return;

                        ExecuteSpawn(transferFrom, playerRes, lastTransferCount, resourceItem, tweenDestination, out isFinished);
                    });

                m_TransferTween[i] = trensferTween;
            }
        }

        private void ExecuteSpawn(Transform transferFrom, ResourceContainer playerRes, float currentTransfer, TransferData resourceItem, float tweenDestination, out bool isFinished)
        {
            isFinished = currentTransfer == tweenDestination;

            int resourcesToTransfer = (int)currentTransfer;

            if (resourcesToTransfer - resourceItem.LastTransferedResources < 1)
            { 
                return;
            }

            Vector3 startPos = transferFrom.position;
            Vector3 endPos = TransferEndPoint.position;
            float gravity = Physics.gravity.y;
            float maxRotAngle = itemVectorAngleOffset;

            //calculate velocity for gravity movement
            LaunchData predictedVel = MathExt.GetPredictedVelocity(
                           startPos, endPos, gravity, itemMaxHeight);


            TransferParticle?.Play();


            playerRes.AddResource(resourceItem.ResourceType, resourceItem.LastTransferedResources);
            playerRes.RemoveResource(resourceItem.ResourceType, resourcesToTransfer);

            var toRemove = resourceItem.LastTransferedResources;

            resourceItem.LastTransferedResources = resourcesToTransfer;
            m_CurrentRealResources.AddResource(resourceItem.ResourceType, resourcesToTransfer);
            m_CurrentRealResources.RemoveResource(resourceItem.ResourceType, toRemove);

            var resourceObjectInstance = GameObjectPool.GetPoolObject(resourceItem.ResourceType.Resource3dItem);

            resourceObjectInstance.SetActive(true);
            resourceObjectInstance.transform.position = startPos;
            resourceObjectInstance.transform.rotation = UnityEngine.Random.rotation;

            var rb = resourceObjectInstance.GetComponent<Rigidbody>();

            m_TransManager.Transfer3dObject(rb, startPos, predictedVel.initialVelocity, TransferEndPoint, onComplete: () =>
            {
                m_CurrentDelayedResources.AddResource(resourceItem.ResourceType, resourcesToTransfer);
                m_CurrentDelayedResources.RemoveResource(resourceItem.ResourceType, toRemove);

                rb.velocity = Vector3.zero;
                resourceObjectInstance.GetComponent<PoolObject>().Release();

                CheckFinish();
            });
        }

        private List<TransferData> SetupTransferData()
        {
            List<TransferData> transferData = new List<TransferData>();

            foreach (var item in ResourceToOpen.ResourceIterator())
            {
                var playerResources = m_PlayerRes.resources.GetResource(item.Key);
                var alreadyTransfered = m_CurrentRealResources.GetResource(item.Key);
                var requiredResources = item.Value;
                var toTransfer = requiredResources - alreadyTransfered;

                if (playerResources < toTransfer)
                {
                    toTransfer = playerResources;
                }

                if (playerResources > 0 && toTransfer != 0)
                {
                    transferData.Add(new TransferData
                    {
                        LastTransferedResources = 0,
                        toTransfer = toTransfer,
                        ResourceType = item.Key
                    });
                }
            }

            return transferData;
        }

        ZoneTriggerSave ISaveable<ZoneTriggerSave>.Save()
        {
            var zoneData = new ZoneTriggerSave();

            foreach (var item in m_CurrentRealResources.ResourceIterator())
            {
                zoneData.currentResources.Add(new ResourceSaveItem
                {
                    count = item.Value,
                    resourceTypeHash = item.Key.GetId()

                });
            }

            zoneData.finished = m_Finished;

            return zoneData;
        }

        public void Load(ZoneTriggerSave data)
        {
            m_Finished = data.finished;
            gameObject.SetActive(!m_Finished);

            foreach (var item in data.currentResources)
            {
                var resource = m_gResources.Value.FirstOrDefault(e => item.resourceTypeHash == e.GetId());
                m_CurrentRealResources.SetResource(resource, item.count);
                m_CurrentDelayedResources.SetResource(resource, item.count);
            }
        }
    }
}
