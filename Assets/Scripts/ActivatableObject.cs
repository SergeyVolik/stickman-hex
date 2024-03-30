using DG.Tweening;
using UnityEngine;

namespace Prototype
{
    public class ActivatableObject : MonoBehaviour, IActivateable
    {
        private bool m_IsActive;

        public float activationDuration = 0.5f;
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
        }

        public void Deactivate()
        {
            m_IsActive = false;        
            transform.DOScale(0, activationDuration);
        }

        public void DeactivateInstant()
        {
            transform.localScale = Vector3.zero;
        }

        public bool IsActive()
        {
            return m_IsActive;
        } 
    }
}

