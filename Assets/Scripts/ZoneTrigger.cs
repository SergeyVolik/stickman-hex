using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using static UnityEditor.Progress;

namespace Prototype
{
    public class ZoneTrigger : MonoBehaviour
    {
        public GameObject ToOpen;

        public ResourceContainer ResourceToOpen;

        public ResourceView ResourceView;
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

        private void Awake()
        {
            Init();
        }

        [Inject]
        void Construct(PlayerResources resources)
        {
            m_PlayerRes = resources;
        }

        public class TransferData
        {
            public float toTransfer;
            public int LastTransferedResources;
            public int LastTransferedResourcesInAnimationTick;

            public ResourceTypeSO ResourceType;
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerInput>();
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

        private void StartTransfer(Transform transferFrom, List<TransferData> resources)
        {
            m_TransferTween = new TweenerCore<float, float, FloatOptions>[resources.Count];
            float duration = transferDuration;
            var playerRes = m_PlayerRes.resources;
            int defaultTickNumber = maxTransferTicks;

            for (int i = 0; i < resources.Count; i++)
            {
                float currentTransfer = 0;
                float spawnT = 0;

                float ticksPerDuration = resources[i].toTransfer < defaultTickNumber ? math.clamp(resources[i].toTransfer, 0, defaultTickNumber) : defaultTickNumber;
                float spawnInterval = duration / ticksPerDuration;
                var resourceItem = resources[i];
                float tweenDestination = resources[i].toTransfer;
                bool isFinished = false;

                var trensferTween = DOTween.To(() => currentTransfer, (v) => currentTransfer = v, tweenDestination, duration)
                    .SetEase(resourceTransferEase).OnUpdate(() =>
                    {
                        spawnT += Time.deltaTime;
                        bool executeSpawn = false;

                        if (spawnT > spawnInterval)
                        {
                            executeSpawn = true;
                            spawnT = 0;
                        }

                        if (executeSpawn == false)
                            return;

                        ExecuteSpawn(transferFrom, playerRes, currentTransfer, resourceItem, tweenDestination, out isFinished);
                    })
                    .OnComplete(() =>
                    {
                        if (!isFinished)
                        {
                            ExecuteSpawn(transferFrom, playerRes, currentTransfer, resourceItem, tweenDestination, out isFinished);
                        }
                    });

                m_TransferTween[i] = trensferTween;
            }
        }

        private void ExecuteSpawn(Transform transferFrom, ResourceContainer playerRes, float currentTransfer, TransferData resourceItem, float tweenDestination, out bool isFinished)
        {
            var startPos = transferFrom.position;
            var endPos = TransferEndPoint.position;
            var gravity = Physics.gravity.y;
            float maxRotAngle = itemVectorAngleOffset;
            var predictedVel = MathExt.GetPredictedVelocity(
                           startPos, endPos, gravity, itemMaxHeight);

            isFinished = currentTransfer == tweenDestination;

            TransferParticle?.Play();

            var item = resourceItem;
            int resourcesToTransfer = (int)currentTransfer;
            playerRes.AddResource(item.ResourceType, item.LastTransferedResources);
            playerRes.RemoveResource(item.ResourceType, resourcesToTransfer);
            var toAddRes = item.LastTransferedResources;

            item.LastTransferedResources = resourcesToTransfer;
            resourceItem.LastTransferedResourcesInAnimationTick = resourcesToTransfer;
            var objIns = GameObject.Instantiate(item.ResourceType.Resource3dItem, startPos, UnityEngine.Random.rotation);

            var rb = objIns.GetComponent<Rigidbody>();

            rb.angularVelocity =
            new Vector3(
                UnityEngine.Random.Range(-itemRotAngle, itemRotAngle),
                UnityEngine.Random.Range(-itemRotAngle, itemRotAngle),
                UnityEngine.Random.Range(-itemRotAngle, itemRotAngle));

            var angle1 = quaternion.AxisAngle(math.forward(), UnityEngine.Random.Range(math.radians(-maxRotAngle), math.radians(maxRotAngle)));
            var angle2 = quaternion.AxisAngle(math.right(), UnityEngine.Random.Range(math.radians(-maxRotAngle), math.radians(maxRotAngle)));

            var velocityWithOffset = math.mul(angle1, math.mul(angle2, predictedVel.initialVelocity));

            rb.velocity = velocityWithOffset;
            var seuq = DOTween.Sequence();

            seuq.Insert(ItemMoveDelay, objIns.transform.DOMove(TransferEndPoint.position, itemMoveDuration).SetEase(itemMoveEase).OnComplete(() =>
            {
                ResourceToOpen.AddResource(item.ResourceType, toAddRes);
                ResourceToOpen.RemoveResource(item.ResourceType, resourcesToTransfer);

                rb.velocity = Vector3.zero;
                GameObject.Destroy(objIns);
            }));
        }

        private List<TransferData> SetupTransferData()
        {
            List<TransferData> transferData = new List<TransferData>();

            foreach (var item in ResourceToOpen.ResourceIterator())
            {
                var playerResources = m_PlayerRes.resources.GetResource(item.Key);

                var requiredResources = item.Value;
                var toTransfer = requiredResources;
                if (playerResources < requiredResources)
                {
                    toTransfer = playerResources;
                }

                if (playerResources > 0 && requiredResources != 0)
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

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerInput>())
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

            bool finished = ResourceToOpen.IsEmpty();

            if (finished == true)
            {
                ToOpen.gameObject.SetActive(true);
                ToOpen.transform.localScale = Vector3.zero;
                ToOpen.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                gameObject.SetActive(false);
            }
        }

        bool m_Inted = false;
        public void Init()
        {
            if (m_Inted)
                return;

            m_Inted = true;
            ResourceView.Bind(ResourceToOpen);
            m_Camera = Camera.main;
            ToOpen.SetActive(false);
        }
    }
}
