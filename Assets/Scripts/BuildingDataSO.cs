using Unity.Entities;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingDataSO : ScriptableObject
{
    public BuildingType buildingType;
    public Transform prefab;
    public float buildingDistanceMin;

    public Entity GetEntityPrefab(EntitiesReferences entitiesReferences)
    {
        switch (buildingType)
        {
            default:
            case BuildingType.None:
            case BuildingType.ZombieSpawner:
            case BuildingType.Barrack : return entitiesReferences.barrackPrefabEntity;
            case BuildingType.Tower : return entitiesReferences.towerPrefabEntity;
        }
    }
}
