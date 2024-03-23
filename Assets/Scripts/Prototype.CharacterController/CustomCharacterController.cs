using Unity.Mathematics;
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
        public float acceleration;
        public float deceleration;

        public bool IsMoving => moveVector != Vector2.zero;

        public Vector2 moveVector;
        private Vector2 m_LastMoveVector;
        public void SetMoveVector(Vector2 moveInput)
        {
            moveVector = moveInput.normalized;
        }

        private float m_CurrentMoveSpeed;
        private void Awake()
        {
            m_CharCOntroller = GetComponent<CharacterController>();
            m_Transform = transform;
            m_CurrentMoveSpeed = moveSpeed;
        }

     
        private void Update()
        {
            var deltaTime = Time.deltaTime;

            if (moveVector != Vector2.zero)
            {
                m_CurrentMoveSpeed += deltaTime * acceleration;
                m_LastMoveVector = moveVector;
            }
            else m_CurrentMoveSpeed -= deltaTime * deceleration;

            var move3DVector = new Vector3(m_LastMoveVector.x, 0, m_LastMoveVector.y);

            m_CurrentMoveSpeed = math.clamp(m_CurrentMoveSpeed, 0, moveSpeed);

            if(m_LastMoveVector != Vector2.zero)
                m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, Quaternion.LookRotation(move3DVector), rotationSpeed * deltaTime);

            m_CharCOntroller.SimpleMove(move3DVector * m_CurrentMoveSpeed);
        }
    }
}
