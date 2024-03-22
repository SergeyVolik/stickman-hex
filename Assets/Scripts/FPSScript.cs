using UnityEngine;

namespace FBCapture
{
    public class FPSScript : MonoBehaviour
    {
        /// <summary>
        /// Delta time
        /// </summary>
        float deltaTime = 0.0f;

        /// <summary>
        /// It will be used for printing out fps text on screen
        /// </summary>
        public TMPro.TextMeshProUGUI text;
        
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            var msec = deltaTime * 1000.0f;
            var fps = 1.0f / deltaTime;
            //text.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            text.text = string.Format("{0:0.}", fps);
        }
    }
}