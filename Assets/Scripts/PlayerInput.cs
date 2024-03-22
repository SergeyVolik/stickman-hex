using UnityEngine;
using Zenject;

namespace Prototype
{
    public class PlayerInput : MonoBehaviour
    {
        private IInputReader m_Input;
        private CustomCharacterController m_CharController;

        [Inject]
        public void Construct(IInputReader input)
        {
            m_Input = input;
        }

        private void Awake()
        {
            m_CharController = GetComponent<CustomCharacterController>();
        }

        private void Update()
        {
            var moveInput = m_Input.ReadMoveInput().normalized;
            m_CharController.SetMoveVector(moveInput);
        }
    }
}
