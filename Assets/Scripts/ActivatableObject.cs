using DG.Tweening;
using System;
using UnityEngine;

namespace Prototype
{
    public class ActivatableObject : MonoBehaviour, IActivateable
    {
        private bool m_IsActive;

        public float activationDuration = 0.5f;

        public event Action onActivated = delegate { };
        public event Action onDeactivated = delegate { };

        private void Awake()
        {
            m_IsActive = gameObject.activeSelf;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            m_IsActive = true;
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, activationDuration).SetEase(Ease.OutBack);

            onActivated.Invoke();
        }

        public void Deactivate()
        {
            m_IsActive = false;
            transform.DOScale(0, activationDuration).OnComplete(() => {
                gameObject.SetActive(false);
            });

            onDeactivated.Invoke();
        }

        public void DeactivateInstant()
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;

            onDeactivated.Invoke();
        }

        public void ActivateInstant()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;

            onActivated.Invoke();
        }

        public bool IsActive()
        {
            return m_IsActive;
        }
    }
}

