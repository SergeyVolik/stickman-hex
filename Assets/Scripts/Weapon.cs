using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Prototype
{
    public enum WeaponType
    {
        Axe,
        Crutch
    }

    public class Weapon : MonoBehaviour, IOwnable
    {
        public WeaponType Type;

        [field: SerializeField]
        public GameObject Owner { get; set; }

        public int damage;
        public TrailRenderer Trail;
        private Collider m_HitBox;

        private Transform m_Transform;

        private TweenerCore<Vector3, Vector3, VectorOptions> m_DeactivateTween;

      

        private void Awake()
        {
            m_Transform = transform;
            m_HitBox = GetComponent<Collider>();
           
            ActivateTrail(false);
            m_DeactivateTween.Pause();
        }

        public void ActivateTrail(bool activate)
        {
            Trail.gameObject.SetActive(activate);
            Trail.Clear();
        }

        public void EnableHitBox(bool enable)
        {
            m_HitBox.enabled = enable;
            ActivateTrail(enable);
        }

        public void HideWeapon()
        {
           
            m_DeactivateTween = m_Transform
                .DOScale(Vector3.zero, 0.2f)
                .OnComplete(() => {
               
            }).SetDelay(1f);
        }

        public void ShowWeapon()
        {
            m_DeactivateTween?.Kill();

            float activateDuration = 0.2f;
            m_Transform.DOScale(Vector3.one, activateDuration).SetEase(Ease.InSine).OnComplete(() => {               
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Owner == other.gameObject)
                return;

            if (other.TryGetComponent<FarmableObject>(out var farmable))
            {
                if (farmable.RequiredWeapon == Type)
                {
                    other.GetComponent<IDamageable>().DoDamage(damage, gameObject);
                }
            }
        }
    }
}
