using UnityEngine;
using Zenject;

namespace Prototype
{
    public interface IResourceHolder
    {
        public ResourceContainer Resources { get; }
        public Transform CenterPoint { get; }
    }

    public class PlayerResourceHolder : MonoBehaviour, IResourceHolder
    {
        public ResourceContainer Resources => m_Resources;

        [field: SerializeField]
        public Transform CenterPoint { get; set; }

        private ResourceContainer m_Resources;

        [Inject]
        public void Construct(PlayerResources resources)
        {
            m_Resources = resources.resources;
        }
    }
}
