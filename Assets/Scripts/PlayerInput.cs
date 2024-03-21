using UnityEngine;
using Zenject;

namespace Prototype
{
    public interface ICharacterInput
    {
        public bool IsMoving { get; }

    }

    public class PlayerInput : MonoBehaviour, ICharacterInput
    {
        private IInputReader m_Input;
        private Transform m_Transform;
        private CharacterController m_CharCOntroller;
        public float moveSpeed = 5;
        public float rotationSpeed = 5f;
        public bool IsMoving { get; private set; }

        [Inject]
        public void Construct(IInputReader input)
        {
            m_Input = input;
            m_Transform = transform;
        }

        private void Awake()
        {
            m_CharCOntroller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var moveInput = m_Input.ReadMoveInput().normalized;
            IsMoving = moveInput != Vector2.zero;
          
               

            var moveVector = new Vector3(moveInput.x, 0, moveInput.y);

            var deltaTime = Time.deltaTime;

            if (moveInput != Vector2.zero)
                m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, Quaternion.LookRotation(moveVector), rotationSpeed * deltaTime);

            m_CharCOntroller.SimpleMove(moveVector * moveSpeed);
            //m_CharCOntroller.Move(moveVector * deltaTime * moveSpeed);
        }
    }
}
