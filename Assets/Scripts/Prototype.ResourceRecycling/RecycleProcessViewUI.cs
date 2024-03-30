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

        public void Bind(ResourceRecycling recicling)
        {
            m_Item.SetSprite(recicling.destinationResource.resourceIcon);

            m_recicling = recicling;
            recicling.onProcessUpdatedChanged += Recicling_onChanged;
        }

        private void Recicling_onChanged()
        {
            if (m_PrevValue != m_recicling.itemToRecycle)
            {
                m_Item.SetText(TextUtils.IntToText(m_recicling.itemToRecycle));
            }
            m_Slider.value = m_recicling.GetCurrentProcessProgress();

            m_PrevValue = m_recicling.itemToRecycle;

            if (m_recicling.itemToRecycle == 0)
            {
                Deactivate();
                return;
            }
            else
            {
                Activate();
            }
        }
    }
}