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

            LoadItem(saveObj.HexLocaSave, saveableObjects);
            LoadItem(saveObj.TransformSave, saveableObjects);
            LoadItem(saveObj.GameObjectSave, saveableObjects);
            LoadItem(saveObj.ZoneSave, saveableObjects);
            LoadItem(saveObj.RecyclingSave, saveableObjects);
        }

        private static void LoadItem<T>(Dictionary<SerializableGuid, T> saveObj, IEnumerable<SaveableObject> saveableObjects)
        {
            foreach (var item in saveableObjects)
            {
                var guid = item.guid;

                if (item.TryGetComponent<ISaveable<T>>(out var recSave))
                {
                    if (saveObj.TryGetValue(guid, out var data))
                        recSave.Load(data);
                }
            }
        }

        private static void SaveItem<T>(Dictionary<SerializableGuid, T> saveObj, IEnumerable<SaveableObject> saveableObjects)
        {
            foreach (var item in saveableObjects)
            {
                var guid = item.guid;
                if (item.TryGetComponent<ISaveable<T>>(out var comp))
                {
                    saveObj.Add(guid, comp.Save());
                }
            }
        }

        public static void RemoveSave()
        {
            PlayerPrefs.DeleteKey(SAVE_NAME);
        }

        public static void SaveGameScene()
        {
            var saveableObjects = GameObject.FindObjectsOfType<SaveableObject>(includeInactive: true);

            var save = new SceneSaveData();

            SaveItem(save.HexLocaSave, saveableObjects);
            SaveItem(save.TransformSave, saveableObjects);
            SaveItem(save.GameObjectSave, saveableObjects);
            SaveItem(save.ZoneSave, saveableObjects);
            SaveItem(save.RecyclingSave, saveableObjects);

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