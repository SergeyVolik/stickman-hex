using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype
{
    public class ResourceUIItem : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI itemText;
        public Image spriteImage;
        private Tweener m_Tween;

        public void DoAnimation()
        {
            m_Tween?.Kill();
            transform.localScale = Vector3.one;
            m_Tween = transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);
        }

        public void SetText(string value)
        {
            itemText.text = value;
        }

        public void SetSprite(Sprite sprite, Color color)
        {
            spriteImage.color = color;
            spriteImage.sprite = sprite;
        }
        public void SetSprite(Sprite sprite)
        {
            spriteImage.sprite = sprite;
        }
    }
}
