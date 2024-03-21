using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class ResourceItem
    {
        public int count;
        public ResourceTypeSO resourceType;
    }

    [System.Serializable]
    public class ResourceContainer : IEquatable<ResourceContainer>
    {
        public ResourceItem[] InitResources;

        private Dictionary<ResourceTypeSO, int> ResourceDic = new Dictionary<ResourceTypeSO, int>();

        public IEnumerable<KeyValuePair<ResourceTypeSO, int>> ResourceIterator() => ResourceDic;

        public event Action<ResourceTypeSO, int> onResourceChanged = delegate { };

        public void Init()
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

        public void AddResource(ResourceTypeSO resourceType, int count)
        {
            ResourceDic.TryGetValue(resourceType, out var current);
            current += count;
            SetResource(resourceType, current);
        }

        public void RemoveResource(ResourceTypeSO resourceType, int count)
        {
            ResourceDic.TryGetValue(resourceType, out var current);
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

        public int GetResource(ResourceTypeSO resourceType)
        {
            ResourceDic.TryGetValue(resourceType, out int result);
            return result;
        }

        public bool Equals(ResourceContainer other)
        {
            return false;
        }
    }

    public class PlayerResources : MonoBehaviour
    {
        public ResourceContainer resources;
    }
}
