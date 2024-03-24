using UnityEngine;

namespace Prototype
{
    public interface IOwnable
    {
        public GameObject Owner { get; set; }
    }
}

