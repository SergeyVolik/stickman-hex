using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Prototype
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public List<ResourceSaveItem> playerResources = new List<ResourceSaveItem>();
    }

    [System.Serializable]
    public class ResourceSaveItem
    {
        public int resourceTypeHash;
        public int count;
    }

    public class SaveManager : MonoBehaviour
    {
        private PlayerResources m_Resource;
        private GameResources m_gResources;

        private const string PLAYER_SAVE_KEY = "PLAYER_SAVE_KEY";

        private void Awake()
        {
            Load();
        }

        [Inject]
        void Construct(PlayerResources pResources, GameResources gResources)
        {
            m_Resource = pResources;
            m_gResources = gResources;
        }

        public void Save()
        {
            SaveSceneHelper.SaveGameScene();

            var save = new PlayerSaveData();

            foreach (var item in m_Resource.resources.ResourceIterator())
            {
                save.playerResources.Add(new ResourceSaveItem
                {
                    count = item.Value,
                    resourceTypeHash = item.Key.GetId(),
                });
            }

            PlayerPrefs.SetString(PLAYER_SAVE_KEY, JsonConvert.SerializeObject(save));
        }

        public void Load()
        {
            Debug.Log("Load");

            if (!PlayerPrefs.HasKey(PLAYER_SAVE_KEY))
                return;

            SaveSceneHelper.LoadGameScene();

            var saveData = JsonConvert.DeserializeObject<PlayerSaveData>(PlayerPrefs.GetString(PLAYER_SAVE_KEY));

            m_Resource.resources.Clear();

            foreach (var item in saveData.playerResources)
            {
                var resType = m_gResources.Value.FirstOrDefault(e => e.GetId() == item.resourceTypeHash);
                m_Resource.resources.SetResource(resType, item.count);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true)
            {
                Save();
                Debug.Log("OnApplicationPause Save");
            }
        }

        private void OnApplicationQuit()
        {
            Save();
            Debug.Log("OnApplicationQuit Save");
        }

        public void RemoveSaves()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}