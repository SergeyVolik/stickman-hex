using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Prototype
{
    public class TransferMoveManager : MonoBehaviour
    {
        private class TransferItem
        {
            public float moveDelay;
            public float currentTime;
            public float moveDuration;
            public Transform moveableObject;
            public Transform targetPoint;
            public Action onCompletedCallback;
            public EaseFunction easeFunction;
            public Vector3 startPos;
        }

        private List<TransferItem> m_Items;
        private List<TransferItem> m_ToRemove;
        const int INIT_CAP = 100;

        private void Awake()
        {
            m_Items = new List<TransferItem>(INIT_CAP);
            m_ToRemove = new List<TransferItem>();
        }

        public void Transfer3dObject(
           Rigidbody rb,
           Vector3 startPosition,
           Vector3 initialVelocity,
           Transform targetPoint,
           float maxItemInnerRotAngle = 10,
           float maxVectorAngleOffset = 10,
           float moveDelay = 1f,
           float moveDuration = 1f,
           Action onComplete = null
           )
        {
            rb.transform.position = startPosition;

            rb.angularVelocity = new Vector3(
                UnityEngine.Random.Range(-maxItemInnerRotAngle, maxItemInnerRotAngle),
                UnityEngine.Random.Range(-maxItemInnerRotAngle, maxItemInnerRotAngle),
                UnityEngine.Random.Range(-maxItemInnerRotAngle, maxItemInnerRotAngle));

            //rotate RigidBody vector by angles
            var angle1 = quaternion.AxisAngle(math.forward(), UnityEngine.Random.Range(math.radians(-maxVectorAngleOffset), math.radians(maxVectorAngleOffset)));
            var angle2 = quaternion.AxisAngle(math.right(), UnityEngine.Random.Range(math.radians(-maxVectorAngleOffset), math.radians(maxVectorAngleOffset)));
            var velocityWithOffset = math.mul(angle1, math.mul(angle2, initialVelocity));

            rb.velocity = velocityWithOffset;

            var item = new TransferItem()
            {
                moveableObject = rb.transform,
                moveDelay = moveDelay,
                moveDuration = moveDuration,
                onCompletedCallback = onComplete,
                targetPoint = targetPoint,
                easeFunction = EaseManager.ToEaseFunction(Ease.OutSine)
            };

            m_Items.Add(item);
        }

        private void Update()
        {
            var deltaTIme = Time.deltaTime;

            for (int i = 0; i < m_Items.Count; i++)
            {
                var item = m_Items[i];

                item.moveDelay -= deltaTIme;

                if (item.moveDelay <= 0)
                {
                    item.currentTime += Time.deltaTime / item.moveDuration;
                }
                else
                {
                    item.startPos = item.moveableObject.position;
                }

                if (item.currentTime > item.moveDuration)
                {
                    m_ToRemove.Add(item);
                }

                item.moveableObject.position = Vector3.Lerp(item.startPos, item.targetPoint.position, item.easeFunction(item.currentTime, item.moveDuration, 0, 0));
            }

            foreach (var item in m_ToRemove)
            {
                item.onCompletedCallback?.Invoke();
                m_Items.Remove(item);
            }

            m_ToRemove.Clear();
        }
    }
}
