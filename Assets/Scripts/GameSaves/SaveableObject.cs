using UnityEngine;

[DisallowMultipleComponent]
public class SaveableObject : MonoBehaviour
{
    public SerializableGuid guid;

    public bool savePosition = true;
    public bool saveActiveState = true;

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
