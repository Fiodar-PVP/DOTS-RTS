using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBarrackUI : MonoBehaviour
{
    [SerializeField] private Button soldierButton;
    [SerializeField] private Button scoutButton;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private RectTransform unitQueueContainer;
    [SerializeField] private RectTransform unitQueueTemplate;

    private EntityManager entityManager;
    private Entity buildingBarrackEntity;

    private void Awake()
    {
        soldierButton.onClick.AddListener(() =>
        {
            entityManager.SetComponentData(buildingBarrackEntity, new BuildingBarrackUnitEnqueue { unitType = UnitType.Soldier });
            entityManager.SetComponentEnabled<BuildingBarrackUnitEnqueue>(buildingBarrackEntity, true);
        });

        scoutButton.onClick.AddListener(() =>
        {
            entityManager.SetComponentData(buildingBarrackEntity, new BuildingBarrackUnitEnqueue { unitType = UnitType.Scout });
            entityManager.SetComponentEnabled<BuildingBarrackUnitEnqueue>(buildingBarrackEntity, true);
        });

        unitQueueTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        UnitSelectionManager.Instance.OnSelectedChanged += UnitSelectionManager_OnSelectedChanged;
        DOTSEventManager.Instance.OnBuildingBarrackUnitQueueChanged += DOTSEventManager_OnUnitQueueChanged;

        Hide();    
    }

    private void DOTSEventManager_OnUnitQueueChanged(object sender, System.EventArgs e)
    {
        if((Entity)sender == buildingBarrackEntity)
        {
            UpdateUnitQueueVisual();
        }
    }

    private void Update()
    {
        UpdateProgressBarVisual();
    }

    private void UnitSelectionManager_OnSelectedChanged(object sender, System.EventArgs e)
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<BuildingBarrack, Selected>().Build(entityManager);

        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);

        if (entityArray.Length > 0 )
        {
            //Barrack is selected
            buildingBarrackEntity = entityArray[0];

            Show();
            UpdateProgressBarVisual();
            UpdateUnitQueueVisual();
        }
        else
        {
            buildingBarrackEntity = Entity.Null;

            Hide();
        }
    }

    private void UpdateProgressBarVisual()
    {
        if(buildingBarrackEntity == Entity.Null)
        {
            progressBarImage.fillAmount = 0;
            return;
        }

        BuildingBarrack buildingBarrack = entityManager.GetComponentData<BuildingBarrack>(buildingBarrackEntity);

        if(buildingBarrack.activeUnitType == UnitType.None )
        {
            progressBarImage.fillAmount = 0;
        }
        else
        {
            progressBarImage.fillAmount = buildingBarrack.progress / buildingBarrack.progressMax;
        }
    }

    private void UpdateUnitQueueVisual()
    {
        foreach(Transform child in unitQueueContainer)
        {
            if(child == unitQueueTemplate)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer = entityManager.GetBuffer<SpawnUnitTypeBuffer>(buildingBarrackEntity, true);

        foreach (SpawnUnitTypeBuffer spawnUnitTypeBuffer in spawnUnitTypeDynamicBuffer)
        {
            RectTransform unitQueueTemplateTransform = Instantiate(unitQueueTemplate, unitQueueContainer);
            unitQueueTemplateTransform.gameObject.SetActive(true);

            UnitDataSO unitDataSO = GameAssets.Instance.unitTypeSOList.GetUnitDataSO(spawnUnitTypeBuffer.unitType);
            unitQueueTemplateTransform.GetComponent<Image>().sprite = unitDataSO.sprite;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
