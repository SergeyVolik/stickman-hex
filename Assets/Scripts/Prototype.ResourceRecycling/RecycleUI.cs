using Prototype;
using UnityEngine;
using UnityEngine.UI;

public class RecycleUI : ActivatableUI
{
    public Slider m_Slider;
    public Slider m_SliderView;
    public Button m_TransferButton;

    public ResourceUIItem sourceResourceUI;
    public ResourceUIItem destionationResourceUI;
    public float m_ViewInitValue;

    protected override void Awake()
    {
        base.Awake();
        m_ViewInitValue = m_SliderView.value;
    }
}
