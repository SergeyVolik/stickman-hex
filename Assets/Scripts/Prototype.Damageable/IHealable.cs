using UnityEngine;

namespace Prototype
{
    public interface IHealable
    {
        public void DoHeal(int heal, GameObject source);
    }
}
