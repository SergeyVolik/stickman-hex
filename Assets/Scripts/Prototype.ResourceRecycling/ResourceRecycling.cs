using Prototype;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ResourceRecycling : MonoBehaviour
{
    public ResourceTypeSO sourceResource;
    public ResourceTypeSO destinationResource;

    [Min(1)]
    public int itemsToDestResource = 1;

    private PlayerResources m_PlayerResources;
    private IPlayerFactory m_PlayerFactory;
    private WorldToScreenUIManager m_wtsManager;
    private ActivateByDistanceToPlayerManager m_actManager;
    [SerializeField]
    private RecycleUI m_UIPrefab;

    private RecycleUI m_UIInstance;
    private WordlToScreenItem m_WorldToScreenHandle;
    private ActivateableByDistance m_ActByDistHandle;
    [SerializeField]
    private Transform m_UiBindPoint;

    private ResourceUIItem m_SourceResourceUI;
    private ResourceUIItem m_DestionationResourceUI;

    public float distanceToActivateUI = 2f;
    [Inject]
    public void Construct(
        PlayerResources resources,
        IPlayerFactory playerFactory,
        WorldToScreenUIManager wtsManager,
        ActivateByDistanceToPlayerManager actManager)
    {
        m_PlayerResources = resources;
        m_PlayerFactory = playerFactory;
        m_wtsManager = wtsManager;
        m_actManager = actManager;
    }

    private void Awake()
    {
        m_UIInstance = GameObject.Instantiate(m_UIPrefab, m_wtsManager.Root);
    }

    private void OnEnable()
    {
        m_WorldToScreenHandle = m_wtsManager.Register(new WordlToScreenItem
        {
            worldPositionTransform = m_UiBindPoint,
            item = m_UIInstance.GetComponent<RectTransform>()
        });

        m_ActByDistHandle = m_actManager.Register(new ActivateableByDistance
        {
            DistanceObj = m_UiBindPoint,
            DistanceToActivate = distanceToActivateUI,
            ItemToActivate = m_UIInstance
        });
    }

    private void OnDisable()
    {
        m_wtsManager.Unregister(m_WorldToScreenHandle);
        m_actManager.Unregister(m_ActByDistHandle);
    }
}
