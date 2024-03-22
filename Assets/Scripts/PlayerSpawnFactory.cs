using System;
using UnityEngine;
using Zenject;

namespace Prototype
{
    public class PlayerSpawnFactory
    {
        public event Action<GameObject> onPlayerSpawned = delegate { };

        private GameObject m_PlayerPrefab;
        private DiContainer m_container;
        private GameObject m_LastSpawnedPlayer;
        public GameObject CurrentPlayerUnit => m_LastSpawnedPlayer;

        public PlayerSpawnFactory(GameObject playerPrefab, DiContainer container)
        {
            m_PlayerPrefab = playerPrefab;
            m_container = container;
        }

        public void SpawnAtPosition(Vector3 spawnPos)
        {         
            m_LastSpawnedPlayer = m_container.InstantiatePrefab(m_PlayerPrefab, spawnPos, Quaternion.identity, null);
            onPlayerSpawned.Invoke(m_LastSpawnedPlayer);
        }
    }
}

