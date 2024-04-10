using Newtonsoft.Json;
using UnityEngine;

namespace Prototype
{
    public abstract class PlayerPrefsSaveManager<T> : MonoBehaviour, ISaveManager<T> where T : class, new()
    {
        public void SerializedData(T data, string key)
        {
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));
        }

        public T DerializedData(string key)
        {
            var data = JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key));
            return data;
        }

        public void Save(string key)
        {
            var saveData = new T();

            SavePass(saveData);

            SerializedData(saveData, key);
        }

        public void Load(string key)
        {
            var loadData = DerializedData(key);

            LoadPass(loadData);
        }

        public abstract void SavePass(T saveData);
        public abstract void LoadPass(T LoadData);
    }
}