using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public struct LaunchData
    {
        public readonly float3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(float3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }

        public override string ToString()
        {
            return $"initialVelocity: {initialVelocity} timeToTarget: {timeToTarget}";
        }

    }

    public static class PrototypeMath
    {
        public static float3 orthogonal(float3 v)
        {
            float x = math.abs(v.x);
            float y = math.abs(v.y);
            float z = math.abs(v.z);

            float3 other = x < y ? (x < z ? math.right() : math.forward()) : (y < z ? math.up() : math.forward());
            return math.cross(v, other);
        }

        public static LaunchData GetPredictedVelocity(float3 startPosition, float3 targetPostion, float gravity, float maxHeight)
        {
            float displacementY = targetPostion.y - startPosition.y;
            float3 displacementXZ = new float3(targetPostion.x - startPosition.x, 0, targetPostion.z - startPosition.z);
            float time = math.sqrt(-2 * maxHeight / gravity) + math.sqrt(2 * (displacementY - maxHeight) / gravity);
            float3 velocityY = math.up() * math.sqrt(-2 * gravity * maxHeight);
            float3 velocityXZ = displacementXZ / time;

            return new LaunchData(velocityXZ + velocityY * -math.sign(gravity), time);
        }
    }

    public class ZoneTrigger : MonoBehaviour
    {
        public GameObject ToOpen;

        public ResourceContainer ResourceToOpen;
        public ResourceContainer CurrentResource;

        public ResourceView ResourceView;
        private PlayerResources m_PlayerRes;

        public Transform ZoneUI;
        public Transform TransferEndPoint;

        private Camera m_Camera;

        public float transferDuration = 2;
        public int maxTransferPerType = 10;
        public float itmeMoveDelya = 1f;
        public float itemMoveDuration = 1f;
        public float itemRotAngle = 90;
        public float maxHeight = 5f;

        private TweenerCore<float, float, FloatOptions> m_LastTween;
        private TweenerCore<float, float, FloatOptions> m_LastAnimationTween;

        private void Awake()
        {
            ResourceToOpen.Init();
            CurrentResource.Init();
            ResourceView.Bind(ResourceToOpen);
            m_Camera = Camera.main;
        }

        [Inject]
        void Construct(PlayerResources resources)
        {
            m_PlayerRes = resources;
        }

        public class TransferData
        {
            public float toTransfer;
            public int lastTransfer;
            public int lastSpawnValue;

            public ResourceTypeSO type;
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerInput>();
            if (player)
            {
                var transferFrom = player.transform;

                List<TransferData> resources = new List<TransferData>();


                foreach (var item in ResourceToOpen.ResourceIterator())
                {
                    var playerResources = m_PlayerRes.resources.GetResource(item.Key);

                    var requiredResources = item.Value;

                    if (playerResources < requiredResources)
                    {
                        requiredResources = playerResources;
                    }

                    resources.Add(new TransferData
                    {
                        lastTransfer = 0,
                        toTransfer = requiredResources,
                        type = item.Key
                    });
                }

                float duration = 3;
                float t = 0;

                var playerRes = m_PlayerRes.resources;
                float spawnT = 0;
                float spawnInterval = 0.1f;
                int maxSpawn = 4;

                m_LastTween = DOTween.To(() => t, (v) => t = v, 1f, duration).SetEase(Ease.InSine).OnUpdate(() =>
                {
                    spawnT += Time.deltaTime;
                    bool spawn = false;
                    if (spawnT > spawnInterval)
                    {
                        spawn = true;
                    }

                    var startPos = transferFrom.position;
                    var endPos = TransferEndPoint.position;
                    var gravity = Physics.gravity.y;

                    var predictedVel = PrototypeMath.GetPredictedVelocity(
                                   startPos, endPos, gravity, maxHeight);

                    float maxRotAngle = 10;
                    foreach (var item in resources)
                    {
                        var items = (int)(t * item.toTransfer);

                        ResourceToOpen.AddResource(item.type, item.lastTransfer);
                        ResourceToOpen.RemoveResource(item.type, items);
                        playerRes.RemoveResource(item.type, item.lastTransfer);
                        playerRes.AddResource(item.type, items);

                        item.lastTransfer = items;


                        if (spawn)
                        {
                            var toSpawn = items - item.lastSpawnValue;

                            if (toSpawn > maxSpawn)
                            {
                                toSpawn = 1;
                            }

                            for (int i = 0; i < toSpawn; i++)
                            {

                                var objIns = GameObject.Instantiate(item.type.Resource3dItem, startPos, UnityEngine.Random.rotation);
                                var rb = objIns.GetComponent<Rigidbody>();

                                rb.angularVelocity =
                                new Vector3(
                                    UnityEngine.Random.Range(-itemRotAngle, itemRotAngle),
                                    UnityEngine.Random.Range(-itemRotAngle, itemRotAngle),
                                    UnityEngine.Random.Range(-itemRotAngle, itemRotAngle));

                                var angle1 = quaternion.AxisAngle(math.forward(), UnityEngine.Random.Range(math.radians(-maxRotAngle), math.radians(maxRotAngle)));
                                var angle2 = quaternion.AxisAngle(math.right(), UnityEngine.Random.Range(math.radians(-maxRotAngle), math.radians(maxRotAngle)));

                                var vecNew = math.mul(angle1, math.mul(angle2, predictedVel.initialVelocity));

                                rb.mass = 10;
                                rb.velocity = vecNew;
                                var seuq = DOTween.Sequence();

                                seuq.Insert(itmeMoveDelya, objIns.transform.DOMove(TransferEndPoint.position, itemMoveDuration).SetEase(Ease.InSine).OnComplete(() =>
                                {
                                    rb.velocity = Vector3.zero;
                                    GameObject.Destroy(objIns);
                                }));

                            }

                            item.lastSpawnValue = items;
                        }
                    }

                });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerInput>())
            {
                m_LastTween?.Kill();
            }
        }

        private void Update()
        {
            ZoneUI.forward = m_Camera.transform.forward;

            bool finished = ResourceToOpen.IsEmpty();

            if (finished == true)
            {
                ToOpen.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}
