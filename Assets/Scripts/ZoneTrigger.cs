using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Prototype
{    
    public class ZoneTrigger : MonoBehaviour
    {
        public class TransferData
        {
            public float toTransfer;
            public int LastTransferedResources;

            public ResourceTypeSO ResourceType;
        }

        [FormerlySerializedAs("ToOpen")]
        public GameObject NextLocation;
        public ResourceContainer ResourceToOpen;
        private ResourceContainer m_CurrentDelayedResources;
        private ResourceContainer m_CurrentRealResources;

        public RequiredResourceView RequiredResourceView;
        private PlayerResources m_PlayerRes;

        public Transform ZoneUI;
        public Transform TransferEndPoint;
        public ParticleSystem TransferParticle;
        private Camera m_Camera;

        public float transferDuration = 2;
        public float ItemMoveDelay = 1f;
        public float itemMoveDuration = 1f;
        public float itemRotAngle = 90;
        public float itemMaxHeight = 4f;
        public float itemVectorAngleOffset = 10;
        public Ease itemMoveEase;
        public Ease resourceTransferEase;
        public int maxTransferTicks = 30;

        private TweenerCore<float, float, FloatOptions>[] m_TransferTween;

        [Inject]
        void Construct(PlayerResources resources)
        {
            m_PlayerRes = resources;
        }

        private void Awake()
        {
            Init();
        }

        bool m_Inted = false;
        public void Init()
        {
            if (m_Inted)
                return;

            m_CurrentDelayedResources = new ResourceContainer();
            m_CurrentRealResources = new ResourceContainer();

            m_Inted = true;
            RequiredResourceView.Bind(ResourceToOpen, m_CurrentDelayedResources);

            m_Camera = Camera.main;
            NextLocation.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacterInput>();
            if (player)
            {
                var transferFrom = player.GetComponent<UnitCenter>().Center;

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
                if (m_TransferTween != null)
                {
                    foreach (var item in m_TransferTween)
                    {
                        item?.Kill();
                    }
                }
            }
        }

        private void Update()
        {
            ZoneUI.forward = m_Camera.transform.forward;

            bool finished = ResourceToOpen.Equals(m_CurrentDelayedResources);

            if (finished == true)
            {
                NextLocation.gameObject.SetActive(true);
                NextLocation.transform.localScale = Vector3.zero;
                NextLocation.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                gameObject.SetActive(false);
            }
        }

        private void StartTransfer(Transform transferFrom, List<TransferData> transferList)
        {
            m_TransferTween = new TweenerCore<float, float, FloatOptions>[transferList.Count];

            float duration = transferDuration;
            var playerRes = m_PlayerRes.resources;
            int defaultTickNumber = maxTransferTicks;

            //start transfer for each required resource
            for (int i = 0; i < transferList.Count; i++)
            {
                float lastTransferCount = 1;
                float spawnTicksPerTransfer = transferList[i].toTransfer < defaultTickNumber ? math.clamp(transferList[i].toTransfer, 0, defaultTickNumber) : defaultTickNumber;
                float spawnInterval = duration / (spawnTicksPerTransfer == 1 ? 1f : (spawnTicksPerTransfer - 1));
                TransferData resourceItem = transferList[i];
                float tweenDestination = transferList[i].toTransfer;
                bool isFinished = false;
                float spawnT = spawnInterval;
                bool executeSpawn = lastTransferCount == transferList[i].toTransfer;

                var trensferTween = DOTween.To(() => lastTransferCount, (newTransferCount) => lastTransferCount = newTransferCount, tweenDestination, duration)
                    .SetEase(resourceTransferEase).OnUpdate(() =>
                    {                      
                        if (spawnT > spawnInterval)
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
            Vector3 startPos = transferFrom.position;
            Vector3 endPos = TransferEndPoint.position;
            float gravity = Physics.gravity.y;
            float maxRotAngle = itemVectorAngleOffset;

            //calculate velocity for gravity movement
            LaunchData predictedVel = MathExt.GetPredictedVelocity(
                           startPos, endPos, gravity, itemMaxHeight);

            isFinished = currentTransfer == tweenDestination;

            TransferParticle?.Play();
          
            int resourcesToTransfer = (int)currentTransfer;
            playerRes.AddResource(resourceItem.ResourceType, resourceItem.LastTransferedResources);
            playerRes.RemoveResource(resourceItem.ResourceType, resourcesToTransfer);
            var toRemove = resourceItem.LastTransferedResources;

            resourceItem.LastTransferedResources = resourcesToTransfer;

            var resourceObjectInstance = GameObjectPool.GetPoolObject(resourceItem.ResourceType.Resource3dItem);

            resourceObjectInstance.SetActive(true);
            resourceObjectInstance.transform.position = startPos;
            resourceObjectInstance.transform.rotation = UnityEngine.Random.rotation;
           
            var rb = resourceObjectInstance.GetComponent<Rigidbody>();


            rb.angularVelocity = new Vector3(
                UnityEngine.Random.Range(-itemRotAngle, itemRotAngle),
                UnityEngine.Random.Range(-itemRotAngle, itemRotAngle),
                UnityEngine.Random.Range(-itemRotAngle, itemRotAngle));

            //rotate RigidBody vector by angles
            var angle1 = quaternion.AxisAngle(math.forward(), UnityEngine.Random.Range(math.radians(-maxRotAngle), math.radians(maxRotAngle)));
            var angle2 = quaternion.AxisAngle(math.right(), UnityEngine.Random.Range(math.radians(-maxRotAngle), math.radians(maxRotAngle)));
            var velocityWithOffset = math.mul(angle1, math.mul(angle2, predictedVel.initialVelocity));

            rb.velocity = velocityWithOffset;
            var seuq = DOTween.Sequence();

            m_CurrentRealResources.AddResource(resourceItem.ResourceType, resourcesToTransfer);
            m_CurrentRealResources.RemoveResource(resourceItem.ResourceType, toRemove);
            
            //delayed move to end position
            seuq.Insert(ItemMoveDelay, resourceObjectInstance.transform
                .DOMove(TransferEndPoint.position, itemMoveDuration)
                .SetEase(itemMoveEase)
                .OnComplete(() =>
            {
                m_CurrentDelayedResources.AddResource(resourceItem.ResourceType, resourcesToTransfer);
                m_CurrentDelayedResources.RemoveResource(resourceItem.ResourceType, toRemove);             

                rb.velocity = Vector3.zero;
                resourceObjectInstance.GetComponent<PoolObject>().Release();
            }));
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
                if (playerResources < requiredResources)
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
    }
}
