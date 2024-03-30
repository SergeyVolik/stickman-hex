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
        private CharacterController m_CharController;
        public float moveSpeed = 5;
        public float rotationSpeed = 5f;
        public float acceleration;
        public float deceleration;
        public LayerMask groundLayer;
        public BoxCollider wall;
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
            m_CharController = GetComponent<CharacterController>();
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

            if (m_LastMoveVector != Vector2.zero)
                m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, Quaternion.LookRotation(move3DVector), rotationSpeed * deltaTime);

            int rays = 5;
            float offset = 0.1f;
            float currentOffset = offset;
            var selfPos = m_Transform.position;


            float rayDist = 100f;
            bool hasGround = false;

            for (int i = 0; i < rays; i++)
            {
                var nextPosition = selfPos + move3DVector * m_CurrentMoveSpeed * currentOffset;
                nextPosition.y += 1f;

                currentOffset += offset;
                if (Physics.Raycast(nextPosition, Vector3.down, out RaycastHit hit, rayDist, groundLayer))
                {
                    hasGround = true;
                    Debug.DrawLine(nextPosition, hit.point, Color.green, 0, depthTest: false);
                }
                else {
                    Debug.DrawLine(nextPosition, nextPosition + Vector3.down * rayDist, Color.red, 0, depthTest: false);

                }
            }

            if (hasGround)
            {
                m_CharController.SimpleMove(move3DVector * m_CurrentMoveSpeed);
            }
        }
    }
}
