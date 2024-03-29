using System;
using UnityEngine;

namespace Prototype
{
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator m_Animator;
        private ICharacterController m_CharController;

        private static readonly int MoveInputHash = Animator.StringToHash("MoveInput");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int IsAttackingBoolHash = Animator.StringToHash("IsAttacking");
   
        public event Action onBeginAttack = delegate { };
        public event Action onEndAttack = delegate { };
        public event Action onEnableHitBox = delegate { };
        public event Action onDisableHitBox = delegate { };

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_CharController = GetComponentInParent<ICharacterController>();
        }

        private void Update()
        {
            bool isMoving = m_CharController.IsMoving;

            m_Animator.SetFloat(MoveInputHash, isMoving ? 1f : 0f);
            m_Animator.SetBool(IsMovingHash, isMoving);
        }

        public void AttackTrigger()
        {
            m_Animator.SetTrigger(AttackTriggerHash);
            m_Animator.SetBool(IsAttackingBoolHash, true);
        }

        public void ResetAttack()
        {
            m_Animator.ResetTrigger(AttackTriggerHash);
            m_Animator.SetBool(IsAttackingBoolHash, false);
        }

        public void OnBeginAttack()
        {
            onBeginAttack.Invoke();
            m_Animator.SetBool(IsAttackingBoolHash, true);
        }

        public void OnEndAttack()
        {
            onEndAttack.Invoke();
            m_Animator.SetBool(IsAttackingBoolHash, false);
        }

        public void OnEnableHitBox()
        {
            onEnableHitBox.Invoke();
        }

        public void OnDisableHitBox()
        {
            onDisableHitBox.Invoke();
        }
    }
}
