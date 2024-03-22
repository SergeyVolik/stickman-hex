using UnityEngine;

namespace Prototype
{
    public interface ICharacterController
    {
        public bool IsMoving { get; }
        public void SetMoveVector(Vector2 moveInput);
    }

    public class CustomCharacterController : MonoBehaviour, ICharacterController
    {
        private Transform m_Transform;
        private CharacterController m_CharCOntroller;
        public float moveSpeed = 5;
        public float rotationSpeed = 5f;
        public float maxSlapHeight = 10f;
        public float accelerationPower = 1f;
        public float deccelerationPower = 1f;
        public float gravity;
        public bool IsMoving => moveVector != Vector2.zero;

        public Vector2 moveVector;
        public void SetMoveVector(Vector2 moveInput)
        {
            moveVector = moveInput.normalized;
        }

        private void Awake()
        {
            m_CharCOntroller = GetComponent<CharacterController>();
            m_Transform = transform;
        }

        private void Update()
        {
            var move3DVector = new Vector3(moveVector.x, gravity, moveVector.y);

            var deltaTime = Time.deltaTime;

            if (moveVector != Vector2.zero)
                m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, Quaternion.LookRotation(move3DVector), rotationSpeed * deltaTime);

            m_CharCOntroller.SimpleMove(move3DVector * moveSpeed);
        }
    }
}
