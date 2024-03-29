using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public abstract class ActivatableUI : MonoBehaviour, IActivateable
    {
        private Vector3 m_Scale;
        private Transform m_Trans;
        bool m_IsActive = true;
        private TweenerCore<Vector3, Vector3, VectorOptions> m_DeactivateTween;
        private TweenerCore<Vector3, Vector3, VectorOptions> m_ActivateTween;

        protected virtual void Awake()
        {
            m_Trans = transform;
            m_Scale = m_Trans.localScale;
        }

        public void Activate()
        {
            if (m_IsActive)
                return;

            m_IsActive = true;
            gameObject.SetActive(true);
            m_ActivateTween = m_Trans.DOScale(m_Scale, 0.5f).SetEase(Ease.OutBack);

            if(m_DeactivateTween != null)
                m_DeactivateTween.Kill();
        }

        public void Deactivate()
        {
            if (!m_IsActive)
                return;

            m_IsActive = false;
            m_DeactivateTween = m_Trans.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

            if (m_ActivateTween != null)
                m_ActivateTween?.Kill();
        }

        public bool IsActive()
        {
            return m_IsActive;
        }
    }
}
