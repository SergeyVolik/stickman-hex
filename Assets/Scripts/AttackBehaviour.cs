using Prototype;
using UnityEngine;

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
    }

    private void M_CharAnimator_onEnableHitBox()
    {
        m_CurrentWeapon.EnableHitBox(true);
    }

    private void M_CharAnimator_onEndAttack()
    {
        m_CurrentWeapon.ActivateTrail(false);
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
        if (IsAttacking)
            return;

        var castTrans = m_CastCollider.transform;

        int count = Physics.OverlapBoxNonAlloc(
            castTrans.position + m_CastCollider.center,
            m_CastCollider.size / 2,
            m_CastedColliders,
            castTrans.rotation,
            m_AttackableLayer);

        Debug.Log(count);

        for (int i = 0; i < count; i++)
        {
            var collider = m_CastedColliders[i];

            if (collider.TryGetComponent<FarmableObject>(out var farmableObj))
            {
                m_CurrentWeapon = GetWeaponByType(farmableObj.RequiredWeapon);
                m_CharAnimator.Attack();
            }
        }
    }
}
