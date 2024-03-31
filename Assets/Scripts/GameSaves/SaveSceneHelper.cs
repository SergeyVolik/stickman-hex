using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
    public class SaveSceneHelper
    {
        const string SAVE_NAME = "SCENE1_DEFAULT";

        public static bool HasSave()
        {
            return PlayerPrefs.HasKey(SAVE_NAME);
        }

        public static void LoadGameScene()
        {
            if (!HasSave())
            {
                return;
            }

            var saveableObjects = GameObject.FindObjectsOfType<SaveableObject>(includeInactive: true);

            var saveString = PlayerPrefs.GetString(SAVE_NAME);

            var saveObj = JsonConvert.DeserializeObject<SceneSaveData>(saveString);

            foreach (var item in saveableObjects)
            {
                var guid = item.guid;

                LoadItem(guid, item, saveObj.HexLocaSave);
                LoadItem(guid, item, saveObj.TransformSave);
                LoadItem(guid, item, saveObj.GameObjectSave);
                LoadItem(guid, item, saveObj.ZoneSave);
                LoadItem(guid, item, saveObj.RecyclingSave);              
            }
        }

        private static void LoadItem<T>(SerializableGuid guid, Component item, Dictionary<SerializableGuid, T> saveObj)
        {
            if (item.TryGetComponent<ISaveable<T>>(out var recSave))
            {
                if (saveObj.TryGetValue(guid, out var data))
                    recSave.Load(data);
            }
        }

        private static void SaveItem<T>(SerializableGuid guid, Component item, Dictionary<SerializableGuid, T> saveObj)
        {
            if (item.TryGetComponent<ISaveable<T>>(out var comp))
            {
                saveObj.Add(guid, comp.Save());
            }
        }

        public static void ResetGameSceneSave()
        {
            PlayerPrefs.DeleteKey(SAVE_NAME);
        }

        public static void SaveGameScene()
        {
            var saveableObjects = GameObject.FindObjectsOfType<SaveableObject>(includeInactive: true);

            var save = new SceneSaveData();

            foreach (var item in saveableObjects)
            {
                var guid = item.guid;

                SaveItem(guid, item, save.HexLocaSave);
                SaveItem(guid, item, save.TransformSave);
                SaveItem(guid, item, save.GameObjectSave);
                SaveItem(guid, item, save.ZoneSave);
                SaveItem(guid, item, save.RecyclingSave);          
            }

            PlayerPrefs.SetString(SAVE_NAME, JsonConvert.SerializeObject(save));
        }
    }

    [System.Serializable]
    public class SceneSaveData
    {
        public Dictionary<SerializableGuid, TransformSave> TransformSave = new Dictionary<SerializableGuid, TransformSave>();
        public Dictionary<SerializableGuid, GameObjectSave> GameObjectSave = new Dictionary<SerializableGuid, GameObjectSave>();
        public Dictionary<SerializableGuid, ZoneTriggerSave> ZoneSave = new Dictionary<SerializableGuid, ZoneTriggerSave>();
        public Dictionary<SerializableGuid, ResourceRecyclingSave> RecyclingSave = new Dictionary<SerializableGuid, ResourceRecyclingSave>();
        public Dictionary<SerializableGuid, HexLocationSave> HexLocaSave = new Dictionary<SerializableGuid, HexLocationSave>();
    }
}