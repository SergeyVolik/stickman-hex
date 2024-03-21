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
    public override void InstallBindings()
    {

        Container.Bind<IInputReader>().FromInstance(new PlayerInputReader(joystick));
    }
}