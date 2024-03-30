using Newtonsoft.Json;
using Prototype;
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

                if (item.TryGetComponent<ISaveable<TransformSave>>(out var comp))
                {
                    if (saveObj.TransformSave.TryGetValue(guid, out var data))
                        comp.Load(data);
                }

                if (item.TryGetComponent<ISaveable<GameObjectSave>>(out var goSave))
                {
                    if (saveObj.GameObjectSave.TryGetValue(guid, out var data))                   
                        goSave.Load(data);                   
                }

                if (item.TryGetComponent<ISaveable<ZoneTriggerSave>>(out var zoneSave))
                {
                    if (saveObj.ZoneSave.TryGetValue(guid, out var data))
                        zoneSave.Load(data);
                }
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

                if (item.TryGetComponent<ISaveable<TransformSave>>(out var comp))
                {
                    save.TransformSave.Add(guid, comp.Save());
                }

                if (item.TryGetComponent<ISaveable<GameObjectSave>>(out var goSave))
                {
                    save.GameObjectSave.Add(guid, goSave.Save());
                }

                if (item.TryGetComponent<ISaveable<ZoneTriggerSave>>(out var ztSave))
                {
                    save.ZoneSave.Add(guid, ztSave.Save());
                }
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

    }
}