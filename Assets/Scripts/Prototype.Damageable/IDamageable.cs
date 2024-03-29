using UnityEngine;

namespace Prototype
{
    public interface IDamageable
    {
        public void DoDamage(int damage, GameObject source);
    }
}
