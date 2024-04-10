using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
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

    public class SaveManager : PlayerPrefsSaveManager<PlayerSaveData>
    {
        private PlayerResources m_Resource;
        private GameResources m_gResources;

        private const string PLAYER_SAVE_KEY = "PLAYER_SAVE_KEY";

        [Inject]
        void Construct(PlayerResources pResources, GameResources gResources)
        {
            m_Resource = pResources;
            m_gResources = gResources;
        }

        public override void SavePass(PlayerSaveData save)
        {
            foreach (var item in m_Resource.resources.ResourceIterator())
            {
                save.playerResources.Add(new ResourceSaveItem
                {
                    count = item.Value,
                    resourceTypeHash = item.Key.GetId(),
                });
            }

            SaveSceneHelper.SaveGameScene();
        }

        public override void LoadPass(PlayerSaveData LoadData)
        {
            Debug.Log("Load");

            if (!PlayerPrefs.HasKey(PLAYER_SAVE_KEY))
                return;

            var saveData = JsonConvert.DeserializeObject<PlayerSaveData>(PlayerPrefs.GetString(PLAYER_SAVE_KEY));

            m_Resource.resources.Clear();

            foreach (var item in saveData.playerResources)
            {
                var resType = m_gResources.Value.FirstOrDefault(e => e.GetId() == item.resourceTypeHash);
                m_Resource.resources.SetResource(resType, item.count);
            }

            SaveSceneHelper.LoadGameScene();
        }

        public void Load()
        {
            Load(PLAYER_SAVE_KEY);
        }

        public void Save()
        {
            Save(PLAYER_SAVE_KEY);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true)
            {
                Save(PLAYER_SAVE_KEY);
                Debug.Log("OnApplicationPause Save");
            }
        }

        private void OnApplicationQuit()
        {
            Save(PLAYER_SAVE_KEY);
            Debug.Log("OnApplicationQuit Save");
        }

        public void RemoveSaves()
        {
            if (PlayerPrefs.HasKey(PLAYER_SAVE_KEY))
            {
                PlayerPrefs.DeleteKey(PLAYER_SAVE_KEY);
                SaveSceneHelper.RemoveSave();
            }
        }
    }
}