using DG.Tweening;
using UnityEngine;

namespace Prototype
{
    public class HexLocation : MonoBehaviour
    {
        [SerializeField]
        private ActivatableObject[] m_Activateables;

        public float activationDelays = 0.5f;
        public void Activate()
        {
            gameObject.SetActive(true);

            foreach (var item in m_Activateables)
            {
                if (item == null)
                    continue;

                item.DeactivateInstant();
            }

            float inserT = 0f;          

            foreach (var item in m_Activateables)
            {
                if (item == null)
                    continue;

                DOVirtual.DelayedCall(inserT, item.Activate);
                inserT += activationDelays;
            }
        }
    }
}
