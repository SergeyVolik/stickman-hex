using Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype
{
    public class Portal : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerCharacterInput>())
            {
                SaveSceneHelper.ResetGameSceneSave();
                SceneManager.LoadScene(0);
            }
        }
    }
}
