using UnityEngine;

namespace Prototype
{
    public class AttackBehaviour : MonoBehaviour
    {
        public Weapon[] weapons;

        [SerializeField]
        private CharacterAnimator m_CharAnimator;
        private Transform m_Transform;

        [SerializeField]
        public LayerMask m_AttackableLayer;
        private Collider[] m_CastedColliders;
        [SerializeField]
        public BoxCollider m_CastCollider;

        private void Awake()
        {
            m_Transform = transform;
            foreach (Weapon weapon in weapons)
            {
                weapon.transform.localScale = Vector3.zero;
                weapon.Owner = gameObject;
            }

            m_CastedColliders = new Collider[20];

            m_CharAnimator.onBeginAttack += M_CharAnimator_onBeginAttack;
            m_CharAnimator.onEndAttack += M_CharAnimator_onEndAttack;
            m_CharAnimator.onEnableHitBox += M_CharAnimator_onEnableHitBox;
            m_CharAnimator.onDisableHitBox += M_CharAnimator_onDisableHitBox;
        }

        private void M_CharAnimator_onDisableHitBox()
        {
            m_CurrentWeapon.EnableHitBox(false);
            m_CurrentWeapon.HideWeapon();
        }

        private void M_CharAnimator_onEnableHitBox()
        {
            m_CurrentWeapon.ShowWeapon();
            m_CurrentWeapon.EnableHitBox(true);
        }

        private void M_CharAnimator_onEndAttack()
        {
            IsAttacking = false;
        }

        private void M_CharAnimator_onBeginAttack()
        {
            m_CurrentWeapon.ShowWeapon();
        }

        private Weapon GetWeaponByType(WeaponType type)
        {
            foreach (var item in weapons)
            {
                if (item.Type == type)
                    return item;
            }

            return null;
        }

        private Weapon m_CurrentWeapon;
        private bool IsAttacking;

        private void FixedUpdate()
        {
            //if (IsAttacking)
            //    return;

            var castTrans = m_CastCollider.transform;

            var transScale = m_CastCollider.transform.lossyScale;
            var boxSize = m_CastCollider.size;

            var size = new Vector3(boxSize.x * transScale.x, boxSize.y * transScale.y, boxSize.z * transScale.z);
            var halfSize = size / 2f;

            int count = Physics.OverlapBoxNonAlloc(
                castTrans.position + m_CastCollider.center,
                halfSize,
                m_CastedColliders,
                castTrans.rotation,
                m_AttackableLayer);

            for (int i = 0; i < count; i++)
            {
                var collider = m_CastedColliders[i];

                if (collider.TryGetComponent<FarmableObject>(out var farmableObj))
                {
                    IsAttacking = true;
                    m_CurrentWeapon = GetWeaponByType(farmableObj.RequiredWeapon);
                    m_CharAnimator.AttackTrigger();
                }

            }

            if (count == 0)
            {
                m_CharAnimator.ResetAttack();
            }
        }
    }

}