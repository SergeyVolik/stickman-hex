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
    public override void InstallBindings()
    {
        Container.Bind<IInputReader>().FromInstance(new PlayerInputReader(joystick));
        Container.Bind<PlayerResources>().FromInstance(playerResources);
        playerResourcesView.Bind(playerResources.resources);

        foreach (var item in FindObjectsOfType<ZoneTrigger>())
        {
            item.Init();
        } 
    }
}