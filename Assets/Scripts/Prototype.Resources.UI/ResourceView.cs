using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public class ResourceView : MonoBehaviour
    {
        private ResourceContainer m_Resources;

        public GameObject m_ResourceUIItemPrefab;

        public Dictionary<ResourceTypeSO, ResourceUIItem> uiItems = new Dictionary<ResourceTypeSO, ResourceUIItem>();

        public void Bind(ResourceContainer resources)
        {
            m_Resources = resources;
            Setup();
            m_Resources.onResourceChanged += UpdateResourceUI;
        }

        private void UpdateResourceUI(ResourceTypeSO arg1, int arg2)
        {
            if (uiItems.TryGetValue(arg1, out var item))
            {
                item.SetText(arg2.ToString());
            }
            else
            {
                SetupUIItem(arg1, arg2);
            }
        }

        private void Setup()
        {
            foreach (var item in m_Resources.ResourceIterator())
            {
                SetupUIItem(item.Key, item.Value);
            }
        }

        private void SetupUIItem(ResourceTypeSO type, int count)
        {
            var uiItem = GameObject
                .Instantiate(m_ResourceUIItemPrefab, transform)
                .GetComponent<ResourceUIItem>();

            uiItem.SetText(count.ToString());
            uiItem.SetSprite(type.resourceIcon, type.resourceColor);
            uiItems.Add(type, uiItem);
        }
    }
}
