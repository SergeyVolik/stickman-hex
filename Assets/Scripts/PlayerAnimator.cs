using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator m_Animator;
        private ICharacterController m_CharController;

        private static readonly int MoveInputHash = Animator.StringToHash("MoveInput");

        private void Awake()
        {
            m_CharController = GetComponent<ICharacterController>();
        }

        private void Update()
        {
            m_Animator.SetFloat(MoveInputHash, m_CharController.IsMoving ? 1f : 0f);
        }
    }
}
