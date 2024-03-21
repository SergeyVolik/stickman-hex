using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class ResourceItem
    {
        public int count;
        public ResourceTypeSO resourceType;
    }

    public class PlayerResources : MonoBehaviour
    {
        public ResourceItem[] InitResources;

        private Dictionary<ResourceTypeSO, int> ResourceDic = new Dictionary<ResourceTypeSO, int>();

        public IEnumerable<KeyValuePair<ResourceTypeSO, int>> ResourceIterator() => ResourceDic;

        private void Awake()
        {
            ResourceDic.Clear();
            foreach (ResourceItem item in InitResources)
            {
                ResourceDic.Add(item.resourceType, item.count);
            }
        }

        public void SetResource(ResourceTypeSO resourceType, int count)
        {
            ResourceDic[resourceType] = count;
            onResourceChanged.Invoke(resourceType, count);
        }

        public int GetResource(ResourceTypeSO resourceType)
        {
            ResourceDic.TryGetValue(resourceType, out int result);
            return result;
        }

        public event Action<ResourceTypeSO, int> onResourceChanged = delegate { };
    }
}
