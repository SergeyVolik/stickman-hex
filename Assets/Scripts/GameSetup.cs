using Prototype;
using UnityEngine;
using Zenject;

public interface IInputReader
{
    public Vector2 ReadMoveInput();
}

public class PlayerInputReader : IInputReader
{
    public Joystick m_Stick;

    public PlayerInputReader(Joystick stick)
    {
        m_Stick = stick;
    }
    public Vector2 ReadMoveInput()
    {
        return m_Stick.Direction;
    }
}

public class GameSetup : MonoInstaller
{
    public Joystick joystick;
    public PlayerResources playerResources;
    public ResourceView playerResourcesView;
    public CameraController cameraController;
    public GameObject playerPrefab;

    public override void InstallBindings()
    {
        var playerFactory = new PlayerSpawnFactory(playerPrefab, Container);

        Container.Bind<IInputReader>().FromInstance(new PlayerInputReader(joystick));
        Container.Bind<PlayerResources>().FromInstance(playerResources);
        Container.Bind<PlayerSpawnFactory>().FromInstance(playerFactory);
        Container.Bind<CameraController>().FromInstance(cameraController);

        playerResourcesView.Bind(playerResources.resources);

        foreach (var item in FindObjectsOfType<ZoneTrigger>())
        {
            item.Init();
        }

        playerFactory.SpawnAtPosition(new Vector3(0, 0, 0));
    }
}