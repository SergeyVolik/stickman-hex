using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public class RequiredResourceView : MonoBehaviour
    {
        private ResourceContainer m_CurrentResources;
        private ResourceContainer m_RequiredResources;

        public GameObject m_ResourceUIItemPrefab;

        public Dictionary<ResourceTypeSO, ResourceUIItem> uiItems = new Dictionary<ResourceTypeSO, ResourceUIItem>();

        public void Bind(ResourceContainer required, ResourceContainer current)
        {
            m_CurrentResources = current;
            m_RequiredResources = required;
            Setup();
            m_CurrentResources.onResourceChanged += UpdateResourceUI;
            m_RequiredResources.onResourceChanged += UpdateResourceUI;
        }

        private void UpdateResourceUI(ResourceTypeSO arg1, int arg2)
        {
            uiItems.TryGetValue(arg1, out var item);

            var req = m_RequiredResources.GetResource(arg1);
            var current = m_CurrentResources.GetResource(arg1);
            item.SetText($"<size=100%>{current}<size=50%>/{req}");
        }

        private void Setup()
        {
            foreach (var item in m_RequiredResources.ResourceIterator())
            {
                SetupUIItem(item.Key, item.Value);
            }
        }

        private void SetupUIItem(ResourceTypeSO type, int count)
        {
            var uiItem = GameObject
                .Instantiate(m_ResourceUIItemPrefab, transform)
                .GetComponent<ResourceUIItem>();
            uiItems.Add(type, uiItem);
            uiItem.SetSprite(type.resourceIcon, type.resourceColor);
            UpdateResourceUI(type, count);
            
        }
    }
}
