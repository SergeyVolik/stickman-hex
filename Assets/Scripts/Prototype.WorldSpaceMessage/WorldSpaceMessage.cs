using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

namespace Prototype
{
    public class WorldSpaceMessage : MonoBehaviour
    {
        public float tweenDuration = 0.5f;
        public Ease easeType = Ease.OutSine;
        public float yOffset = 1f;

        [SerializeField]
        private CanvasGroup m_CanvasGroup;
        [SerializeField]
        private TextMeshProUGUI m_Text;

        [SerializeField]
        private Image m_Image;
        private Transform m_Transform;

        public float targetScale = 0.1f;

        public float waitBeforeFade;
        public float fadeDuration;

        Camera m_Camera;
        private Camera Camera
        {
            get
            {
                if (m_Camera == null)
                    m_Camera = Camera.main;
                return m_Camera;
            }
        }
        private void Awake()
        {
            m_Transform = transform;
        }

        public void Show(Vector3 position, string text, Sprite sprite = null)
        {
            gameObject.SetActive(true);

            m_Image.sprite = sprite;
            m_Image.enabled = sprite != null;

            m_Text.text = text;
            m_Transform.position = position;
            m_CanvasGroup.alpha = 1f;
            m_Transform.localScale = Vector3.zero;

            float scaleTime = tweenDuration * 0.2f;
            float moveDuration = tweenDuration * 0.8f;

            var seq = DOTween.Sequence();
            seq.Insert(0, m_Transform.DOMoveY(position.y + yOffset, moveDuration));
            seq.Insert(0, m_Transform.DOScale(targetScale, scaleTime));
            seq.Insert(waitBeforeFade, m_CanvasGroup.DOFade(0, fadeDuration));
            seq.SetEase(easeType);
            seq.Play();
            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

            seq.OnUpdate(() =>
            {
                m_Transform.forward = Camera.transform.forward;
            });
        }
    }
}
