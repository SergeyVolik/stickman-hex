using System;
using UnityEngine;

namespace Prototype
{
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator m_Animator;
        private ICharacterController m_CharController;

        private static readonly int MoveInputHash = Animator.StringToHash("MoveInput");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

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
            m_Animator.SetFloat(MoveInputHash, m_CharController.IsMoving ? 1f : 0f);
        }

        public void Attack()
        {
            m_Animator.SetTrigger(AttackTriggerHash);
        }

        public void OnBeginAttack()
        {
            onBeginAttack.Invoke();
        }

        public void OnEndAttack()
        {
            onEndAttack.Invoke();
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
