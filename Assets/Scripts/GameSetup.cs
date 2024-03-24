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
    public ResourceContainer playerResources;
    public Joystick joystick;
    public ResourceView playerResourcesView;
    public CameraController cameraController;
    public TransferMoveManager transferManager;

    public GameObject playerPrefab;

    public override void InstallBindings()
    {
        var playerFactory = new PlayerSpawnFactory(playerPrefab, Container);
        var pResources = new PlayerResources(playerResources);

        Container.Bind<IInputReader>().FromInstance(new PlayerInputReader(joystick));
        Container.Bind<PlayerResources>().FromInstance(pResources);
        Container.Bind<PlayerSpawnFactory>().FromInstance(playerFactory);
        Container.Bind<CameraController>().FromInstance(cameraController);
        Container.Bind<TransferMoveManager>().FromInstance(transferManager);

        playerResourcesView.Bind(pResources.resources);

        foreach (var item in FindObjectsOfType<ZoneTrigger>())
        {
            item.Init();
        }

        playerFactory.SpawnAtPosition(new Vector3(0, 0, 0));
    }
}