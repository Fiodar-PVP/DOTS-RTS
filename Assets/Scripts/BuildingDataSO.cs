using Unity.Entities;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingDataSO : ScriptableObject
{
    public BuildingType buildingType;
    public float constructionTimerMax;
    public float constructionYOffset;
    public Transform prefab;
    public float buildingDistanceMin;
    public bool shouldShowInBuildingPlacementManager;
    public Sprite sprite;
    public Transform ghostPrefab;
    public ResourceAmount[] buildCostResourceAmountArray;

    public Entity GetEntityPrefab(EntitiesReferences entitiesReferences)
    {
        switch (buildingType)
        {
            default:
            case BuildingType.None:
            case BuildingType.ZombieSpawner:
            case BuildingType.Barrack : return entitiesReferences.barrackPrefabEntity;
            case BuildingType.Tower : return entitiesReferences.towerPrefabEntity;
            case BuildingType.IronHarvester : return entitiesReferences.ironHarvesterPrefabEntity;
            case BuildingType.GoldHarvester : return entitiesReferences.goldHarvesterPrefabEntity;
            case BuildingType.OilHarvester : return entitiesReferences.oilHarvesterPrefabEntity;
        }
    }

    public Entity GetEntityVisualPrefab(EntitiesReferences entitiesReferences)
    {
        switch (buildingType)
        {
            default:
            case BuildingType.None:
            case BuildingType.ZombieSpawner:
            case BuildingType.Barrack: return entitiesReferences.barrackVisualPrefabEntity;
            case BuildingType.Tower: return entitiesReferences.towerVisualPrefabEntity;
            case BuildingType.IronHarvester: return entitiesReferences.ironHarvesterVisualPrefabEntity;
            case BuildingType.GoldHarvester: return entitiesReferences.goldHarvesterVisualPrefabEntity;
            case BuildingType.OilHarvester: return entitiesReferences.oilHarvesterVisualPrefabEntity;
        }
    }

    public bool IsNone()
    {
        return buildingType == BuildingType.None;
    }
}
