using Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Prototype
{
    public class Portal : MonoBehaviour
    {
        private SaveManager m_SaveManager;

        [Inject]
        void Construct(SaveManager manager)
        {
            m_SaveManager = manager;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerCharacterInput>())
            {
                m_SaveManager.RemoveSaves();
                SceneManager.LoadScene(0);
            }
        }
    }
}
