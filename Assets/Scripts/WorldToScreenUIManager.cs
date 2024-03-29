using System.Collections.Generic;
using UnityEngine;

public class WordlToScreenItem
{
    public RectTransform item;
    public Transform worldPositionTransform;
}
public class WorldToScreenUIManager : MonoBehaviour
{
    private List<WordlToScreenItem> m_Items = new List<WordlToScreenItem>(10);

    [SerializeField]
    public RectTransform m_Root;

    public RectTransform Root => m_Root;

    private Camera m_Camera;

    private void Awake()
    {
        m_Camera = Camera.main;
    }

    public WordlToScreenItem Register(WordlToScreenItem item)
    {
        m_Items.Add(item);
        return item;
    }

    public void Unregister(WordlToScreenItem item)
    {
        m_Items.Remove(item);
    }

    private void Update()
    {
        foreach (var item in m_Items)
        {
            var pos = item.worldPositionTransform.position;
            var speenPos = m_Camera.WorldToScreenPoint(pos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)item.item.parent, speenPos, null, out var anchorPos);
            item.item.anchoredPosition = anchorPos;
        }
    }
}
