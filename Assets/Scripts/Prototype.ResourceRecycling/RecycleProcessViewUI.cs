using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype
{
    public class RecycleProcessViewUI : ActivatableUI
    {
        [SerializeField]
        public Slider m_Slider;

        [SerializeField]
        private ResourceUIItem m_Item;
        private ResourceRecycling m_recicling;
        int m_PrevValue;
        private bool m_Started;
        private RectTransform m_Rect;
        private Vector2 m_StartSize;

        protected override void Awake()
        {
            base.Awake();
            m_Rect = m_Slider.GetComponent<RectTransform>();
            m_StartSize = m_Rect.sizeDelta;
            m_Item.itemText.text = "";
        }

        public void Bind(ResourceRecycling recicling)
        {
            m_Item.SetSprite(recicling.destinationResource.resourceIcon);

            m_recicling = recicling;
            recicling.onProcessUpdatedChanged += Recicling_onChanged;
        }

        private void StartProcess()
        {
            m_Started = true;

            var target = m_StartSize;
            target.x += 20;
            m_Rect.DOSizeDelta(target, 0.6f).SetEase(Ease.OutBack);
        }

        private void StopProcess()
        {
            m_Started = false;
            m_Rect.DOSizeDelta(m_StartSize, 0.6f).SetEase(Ease.InBack);
            m_Item.itemText.text = "";
        }

        private void Recicling_onChanged()
        {
            if (m_PrevValue != m_recicling.itemToRecycle)
            {
                m_Item.itemText.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), 0.4f);
                m_Item.SetText(TextUtils.IntToText(m_recicling.itemToRecycle));
            }

            m_Slider.value = m_recicling.GetCurrentProcessProgress();

            m_PrevValue = m_recicling.itemToRecycle;

            if (m_recicling.itemToRecycle == 0)
            {
                StopProcess();
                return;
            }
            else if(m_Started == false)
            {
                StartProcess();
            }
        }
    }
}