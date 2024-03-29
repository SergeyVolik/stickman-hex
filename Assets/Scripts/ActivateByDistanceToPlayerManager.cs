using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public interface IActivateable
    {
        public void Activate();
        public void Deactivate();

        public bool IsActive();
    }

    [System.Serializable]
    public class ActivateableByDistance
    {
        public IActivateable ItemToActivate;
        public Transform DistanceObj;
        public float DistanceToActivate;
    }

    public class ActivateByDistanceToPlayerManager : MonoBehaviour
    {
        private IPlayerFactory m_PlayerFactory;

        [SerializeField]
        private List<ActivateableByDistance> m_Items = new List<ActivateableByDistance>(100);

        [Inject]
        void Construct(IPlayerFactory factory)
        {
            m_PlayerFactory = factory;
        }

        public ActivateableByDistance Register(ActivateableByDistance activateable)
        {
            m_Items.Add(activateable);
            return activateable;
        }

        public void Unregister(ActivateableByDistance handle)
        {
            m_Items.Remove(handle);
        }

        private void Update()
        {
            if (m_PlayerFactory.CurrentPlayerUnit == null)
                return;

            var playerPos = m_PlayerFactory.CurrentPlayerUnit.transform.position;
            playerPos.y = 0;
            foreach (var item in m_Items)
            {
                var itemsPos = item.DistanceObj.position;
                itemsPos.y = 0;

                var dist = Vector3.Distance(itemsPos, playerPos);
                var activetable = item.ItemToActivate;
                var distToActivate = item.DistanceToActivate;

                if (dist < distToActivate && !activetable.IsActive())
                {
                    activetable.Activate();
                }
                else if(dist > distToActivate && activetable.IsActive())
                {
                    activetable.Deactivate();
                }
            }
        }
    }
}
