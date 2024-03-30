using Newtonsoft.Json;
using Prototype;
using System.Collections.Generic;
using UnityEngine;

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

        var saveableObjects = GameObject.FindObjectsOfType<SaveableObject>();

        var saveString = PlayerPrefs.GetString(SAVE_NAME);

        var saveObj = JsonConvert.DeserializeObject<SceneSaveData>(saveString);

        foreach (var item in saveableObjects)
        {
            var guid = item.guid;

            if (item.savePosition)
            {
                if (saveObj.SavedPositions.TryGetValue(guid, out Vector3S pos))
                    item.transform.position = pos;
            }

            if (item.saveActiveState)
            {
                if (saveObj.SavedActiveState.TryGetValue(guid, out bool isActive))
                    item.gameObject.SetActive(isActive);
            }
        }
    }

    public static void ResetGameSceneSave()
    {
        PlayerPrefs.DeleteKey(SAVE_NAME);
    }

    public static void SaveGameScene()
    {
        var saveableObjects = GameObject.FindObjectsOfType<SaveableObject>();

        var save = new SceneSaveData();

        foreach (var item in saveableObjects)
        {
            var guid = item.guid;

            if (item.savePosition)
            {
                save.SavedPositions.Add(guid, item.transform.position);
            }

            if (item.saveActiveState)
            {
                save.SavedActiveState.Add(guid, item.gameObject.activeSelf);
            }
        }

        PlayerPrefs.SetString(SAVE_NAME, JsonConvert.SerializeObject(save));
    }
}

[System.Serializable]
public class SceneSaveData
{
    public Dictionary<SerializableGuid, Vector3S> SavedPositions = new Dictionary<SerializableGuid, Vector3S>();
    public Dictionary<SerializableGuid, bool> SavedActiveState = new Dictionary<SerializableGuid, bool>();
}