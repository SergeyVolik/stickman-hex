using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public abstract class ActivatableUI : MonoBehaviour, IActivateable
    {
        private Vector3 m_Scale;
        private Transform m_Trans;
        bool m_Activate = true;

        private void Awake()
        {
            m_Trans = transform;
            m_Scale = m_Trans.localScale;          
        }

        public void Activate()
        {
            m_Activate = true;
            m_Trans.DOScale(m_Scale, 0.5f).SetEase(Ease.OutBack);
        }

        public void Deactivate()
        {
            m_Activate = false;
            m_Trans.DOScale(0, 0.5f).SetEase(Ease.InBack);
        }

        public bool IsActive()
        {
            return m_Activate;
        }
    }
}
