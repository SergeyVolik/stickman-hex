using UnityEngine;

namespace Prototype
{
    public class DelayedResurrection : MonoBehaviour
    {
        public float resurrectionDuration;
        private float m_T;
        private IResurrectable m_Resurrectable;

        // Start is called before the first frame update

        private void Awake()
        {
            m_Resurrectable = GetComponent<IResurrectable>();
            var killable = GetComponent<IKillable>();
            killable.onDeath += () => enabled = true;
            enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            m_T += Time.deltaTime;

            if (m_T > resurrectionDuration)
            {
                m_T = 0;
                m_Resurrectable.Resurrect();
                enabled = false;
            }
        }
    }
}
