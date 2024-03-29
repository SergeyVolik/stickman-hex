using UnityEngine;

namespace Prototype
{
    public class LookAtCamera : MonoBehaviour
    {
        private Transform m_CameraTrans;
        private Transform m_Transform;

        private void Awake()
        {
            m_CameraTrans = Camera.main.transform;
            m_Transform = transform;
        }

        // Update is called once per frame
        void Update()
        {
            m_Transform.forward = m_CameraTrans.forward;
        }
    }

}
