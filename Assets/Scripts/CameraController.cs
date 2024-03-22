using Cinemachine;
using Prototype;
using UnityEngine;
using Zenject;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;
    [SerializeField] private Transform m_CameraTarget;
    public float lookAheadSpeed;
    public float lookAheadOffset;

    private PlayerSpawnFactory m_PlayerFactory;
    private CinemachineComposer m_Composer;

    [Inject]
    public void Construct(PlayerSpawnFactory factory)
    {
        m_PlayerFactory = factory;
        m_Composer = m_VirtualCamera.GetCinemachineComponent<CinemachineComposer>();
    }

    private void Update()
    {
        if (m_PlayerFactory.CurrentPlayerUnit)
        {
            var uniTrans = m_PlayerFactory.CurrentPlayerUnit.transform;
            var unitPos = uniTrans.position;
            var unitForward = uniTrans.forward;
            var offset = unitForward * lookAheadOffset;

            m_CameraTarget.position = Vector3.Lerp(m_CameraTarget.position, unitPos + offset, Time.deltaTime * lookAheadSpeed);
            
        }
    }
}
