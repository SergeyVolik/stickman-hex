using UnityEngine;

namespace Prototype
{
    [CreateAssetMenu]
    public class ResourceTypeSO : ScriptableObject
    {
        public Sprite resourceIcon;
        public Color resourceColor;
        public GameObject Resource3dItem;

        public int GetId() => name.GetHashCode();
    }
}
