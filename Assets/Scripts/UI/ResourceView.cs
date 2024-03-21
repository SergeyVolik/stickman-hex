using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public class ResourceView : MonoBehaviour
    {
        private ResourceContainer m_Resources;

        public GameObject m_ResourceUIItemPrefab;

        public Dictionary<ResourceTypeSO, PlayerResourceUIItem> uiItems = new Dictionary<ResourceTypeSO, PlayerResourceUIItem>();

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
                item.SetValue(arg2);
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
                .GetComponent<PlayerResourceUIItem>();

            uiItem.SetValue(count);
            uiItem.SetSprite(type.resourceIcon, type.resourceColor);
            uiItems.Add(type, uiItem);
        }
    }
}
