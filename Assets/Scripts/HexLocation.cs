using DG.Tweening;
using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class HexLocationSave
    {
        public bool IsOpened = false;
    }

    public class HexLocation : MonoBehaviour, ISaveable<HexLocationSave>
    {
        public bool IsOpened = false;

        [SerializeField]
        private ActivatableObject[] m_Activateables;
        public GameObject Wall;
        public float activationDelays = 0.5f;

        public void Activate()
        {
            Wall.SetActive(false);
            IsOpened = true;
            float inserT = 0f;

            foreach (var item in m_Activateables)
            {
                if (item == null)
                    continue;

                DOVirtual.DelayedCall(inserT, item.Activate);
                inserT += activationDelays;
            }
        }

        public void ActivateInstant()
        {
            Wall.SetActive(false);

            foreach (var item in m_Activateables)
            {
                item.ActivateInstant();
            }
        }

        public void DeactivateInstant()
        {
            foreach (var item in m_Activateables)
            {
                if (item == null)
                    continue;

                item.DeactivateInstant();
            }

            Wall.SetActive(true);
        }

        public HexLocationSave Save()
        {
            return new HexLocationSave
            {
                IsOpened = IsOpened,
            };
        }

        public void Load(HexLocationSave data)
        {
            if (data.IsOpened)
            {
                ActivateInstant();
                IsOpened = true;
            }
        }
    }
}
