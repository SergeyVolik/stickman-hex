using UnityEngine;

namespace Prototype
{
    public interface ISaveManager<T> where T : class, new()
    {
        public void SerializedData(T data, string key);
        public T DerializedData(string key);
        public void Save(string key);
        public void Load(string key);
        public abstract void SavePass(T saveData);
        public abstract void LoadPass(T LoadData);
    }
}