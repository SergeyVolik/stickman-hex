using UnityEngine;
using Zenject;

namespace Prototype
{
    public interface IResourceHolder
    {
        public ResourceContainer Resources { get; }
    }

    public class PlayerResourceHolder : MonoBehaviour, IResourceHolder
    {
        public ResourceContainer Resources => m_Resources;

        private ResourceContainer m_Resources;

        [Inject]
        public void Construct(PlayerResources resources)
        {
            m_Resources = resources.resources;
        }
    }
}
