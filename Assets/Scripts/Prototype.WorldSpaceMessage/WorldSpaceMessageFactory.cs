using UnityEngine;

namespace Prototype
{
    public class WorldSpaceMessageFactory
    {
        private WorldSpaceMessage m_Prefab;

        public WorldSpaceMessageFactory(WorldSpaceMessage prefab)
        {
            m_Prefab = prefab;
        }

        public void SpawnAtPosition(Vector3 position, string text, Sprite sprite = null)
        {
            var worldMessageInst = GameObjectPool.GetPoolObject(m_Prefab);

            worldMessageInst.Show(position, text, sprite);
        }
    }
}