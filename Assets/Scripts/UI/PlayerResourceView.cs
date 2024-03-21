using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public class PlayerResourceView : MonoBehaviour
    {
        private PlayerResources m_Resources;

        public GameObject m_ResourceUIItemPrefab;

        public Dictionary<ResourceTypeSO, PlayerResourceUIItem> uiItems = new Dictionary<ResourceTypeSO, PlayerResourceUIItem>();

        [Inject]
        public void Construct(PlayerResources resources)
        {
            m_Resources = resources;
        }

        private void Start()
        {
            Setup();

            m_Resources.onResourceChanged += UpdateResourceUI;
        }

        private void UpdateResourceUI(ResourceTypeSO arg1, int arg2)
        {
            if (uiItems.TryGetValue(arg1, out var item))
            {
                item.SetValue(arg2);
            }
        }

        private void Setup()
        {
            foreach (var item in m_Resources.ResourceIterator())
            {
                var uiItem = GameObject
                    .Instantiate(m_ResourceUIItemPrefab, transform)
                    .GetComponent<PlayerResourceUIItem>();

                uiItem.SetValue(item.Value);
                uiItem.SetSprite(item.Key.resourceIcon, item.Key.resourceColor);
                uiItems.Add(item.Key, uiItem);
            }
        }
    }
}
