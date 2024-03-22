using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prototype
{
    [System.Serializable]
    public class ResourceItem
    {
        public int count;
        public ResourceTypeSO resourceType;
    }

    [System.Serializable]
    public class ResourceContainer
    {
        [SerializeField]
        private ResourceItem[] m_InitResources;
        bool m_Inited = false;

        private Dictionary<ResourceTypeSO, int> m_ResourceDic = new Dictionary<ResourceTypeSO, int>();
        public IEnumerable<KeyValuePair<ResourceTypeSO, int>> ResourceIterator() => ResourceDic;

        public event Action<ResourceTypeSO, int> onResourceChanged = delegate { };

        private Dictionary<ResourceTypeSO, int> ResourceDic
        {
            get
            {
                if (!m_Inited)
                {
                    Init();
                }

                return m_ResourceDic;
            }
        }

        private void Init()
        {
            m_Inited = true;
            m_ResourceDic.Clear();
            if (m_InitResources != null)
            {
                foreach (ResourceItem item in m_InitResources)
                {
                    m_ResourceDic.Add(item.resourceType, item.count);
                }
            }
        }

        public void SetResource(ResourceTypeSO resourceType, int count)
        {
            ResourceDic[resourceType] = count;
            onResourceChanged.Invoke(resourceType, count);
        }

        public void AddResource(ResourceTypeSO resourceType, int count)
        {
            ResourceDic.TryGetValue(resourceType, out var current);
            current += count;
            SetResource(resourceType, current);
        }

        public void RemoveResource(ResourceTypeSO resourceType, int count)
        {
            var hasResource = ResourceDic.TryGetValue(resourceType, out var current);

            Assert.IsTrue(hasResource, "Can't remove resource.");
            Assert.IsTrue(current >= count, $"Can't Remove resource {count} > {current}");

            current -= count;
            SetResource(resourceType, current);
        }

        public bool IsEmpty()
        {
            foreach (var item in ResourceIterator())
            {
                if (item.Value != 0)
                    return false;
            }

            return true;
        }

        public void Clear()
        {
            m_ResourceDic.Clear();
        }

        public int GetResource(ResourceTypeSO resourceType)
        {
            m_ResourceDic.TryGetValue(resourceType, out int result);
            return result;
        }
    }
}
