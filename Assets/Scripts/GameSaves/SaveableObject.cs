using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public class GameObjectSave
    {
        public bool activeSelf;
    }

    [System.Serializable]
    public class TransformSave
    {
        public Vector3S position;
        public QuaternionS rotation;
    }

    [DisallowMultipleComponent]
    public class SaveableObject : MonoBehaviour, ISaveable<GameObjectSave>, ISaveable<TransformSave>
    {
        public SerializableGuid guid;

        public bool savePosition = true;
        public bool saveActiveState = true;

        public void Load(GameObjectSave data)
        {
            if (data == null)
                return;

            gameObject.SetActive(data.activeSelf);
        }
        public GameObjectSave Save()
        {
            if (saveActiveState == false)
                return null;

            return new GameObjectSave
            {
                activeSelf = gameObject.activeSelf,
            };
        }

        public void Load(TransformSave data)
        {
            if (data == null)
                return;

            if (savePosition)
            {
                transform.position = data.position;
                transform.rotation = data.rotation;
            }
        }

        TransformSave ISaveable<TransformSave>.Save()
        {
            if (savePosition == false)
                return null;

            return new TransformSave
            {
                position = transform.position,
                rotation = transform.rotation
            };
        }

        private void OnValidate()
        {
            var allItems = FindObjectsOfType<SaveableObject>();

            if (guid == new SerializableGuid())
            {
                guid = System.Guid.NewGuid();
            }

            int count = 0;
            foreach (var item in allItems)
            {
                if (item.guid == guid)
                {
                    count++;
                }
            }

            if (count > 1)
            {
                guid = System.Guid.NewGuid();
            }
        }
    }
}