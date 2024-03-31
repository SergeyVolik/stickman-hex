using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(1.15f, 0.2f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.2f);
    }
}
