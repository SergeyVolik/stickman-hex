using System;

namespace Prototype
{
    public interface IResurrectable
    {
        public event Action onResurrected;
        public bool IsDead { get; }
        public void Resurrect();
    }
}