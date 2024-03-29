using System;

namespace Prototype
{
    public interface IKillable
    {
        public event Action onDeath;
        public void Kill();
        public bool IsDead { get; }
    }
}
