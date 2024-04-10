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
    public ActivateByDistanceToPlayerManager activateByDistance;
    public WorldToScreenUIManager worldToScreenUI;
    public GameObject playerPrefab;

    public GameResources gameResources;
    public SaveManager saveManager;
    private PlayerSpawnFactory m_playerFactory;
    private PlayerResources m_pResources;

    public WorldSpaceMessage m_SpaceMessagePrefab;

    public override void InstallBindings()
    {
        m_playerFactory = new PlayerSpawnFactory(playerPrefab, Container);
        m_pResources = new PlayerResources(playerResources);
        var messageFactory = new WorldSpaceMessageFactory(m_SpaceMessagePrefab);

        Container.Bind<IInputReader>().FromInstance(new PlayerInputReader(joystick));
        Container.Bind<PlayerResources>().FromInstance(m_pResources);
        Container.Bind<IPlayerFactory>().FromInstance(m_playerFactory);
        Container.Bind<CameraController>().FromInstance(cameraController);
        Container.Bind<TransferMoveManager>().FromInstance(transferManager);
        Container.Bind<ActivateByDistanceToPlayerManager>().FromInstance(activateByDistance);
        Container.Bind<WorldToScreenUIManager>().FromInstance(worldToScreenUI);
        Container.Bind<GameResources>().FromInstance(gameResources);
        Container.Bind<SaveManager>().FromInstance(saveManager);
        Container.Bind<WorldSpaceMessageFactory>().FromInstance(messageFactory);
    }

    private void Awake()
    {
        playerResourcesView.Bind(m_pResources.resources);

        foreach (var item in FindObjectsOfType<ZoneTrigger>(true))
        {
            item.Init();
        }

        m_playerFactory.SpawnAtPosition(new Vector3(0, 0, 0));

        saveManager.Load();
    }
}