using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject shootLightPrefab;
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private GameObject scoutPrefab;

    [SerializeField] private GameObject buildingBarrackPrefab;
    [SerializeField] private GameObject buildingTowerPrefab;
    [SerializeField] private GameObject buildingIronHarvesterPrefab;
    [SerializeField] private GameObject buildingGoldHarvesterPrefab;
    [SerializeField] private GameObject buildingOilHarvesterPrefab;

    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new EntitiesReferences
            {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                zombiePrefabEntity = GetEntity(authoring.zombiePrefab, TransformUsageFlags.Dynamic),
                shootLightPrefabEntity = GetEntity(authoring.shootLightPrefab, TransformUsageFlags.Dynamic),
                soldierPrefabEntity = GetEntity(authoring.soldierPrefab, TransformUsageFlags.Dynamic),
                scoutPrefabEntity = GetEntity(authoring.scoutPrefab, TransformUsageFlags.Dynamic),

                barrackPrefabEntity = GetEntity(authoring.buildingBarrackPrefab, TransformUsageFlags.Dynamic),
                towerPrefabEntity = GetEntity(authoring.buildingTowerPrefab, TransformUsageFlags.Dynamic),
                ironHarvesterPrefabEntity = GetEntity(authoring.buildingIronHarvesterPrefab, TransformUsageFlags.Dynamic),
                goldHarvesterPrefabEntity = GetEntity(authoring.buildingGoldHarvesterPrefab, TransformUsageFlags.Dynamic),
                oilHarvesterPrefabEntity = GetEntity(authoring.buildingOilHarvesterPrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity bulletPrefabEntity;
    public Entity zombiePrefabEntity;
    public Entity shootLightPrefabEntity;
    public Entity soldierPrefabEntity;
    public Entity scoutPrefabEntity;

    public Entity barrackPrefabEntity;
    public Entity towerPrefabEntity;
    public Entity ironHarvesterPrefabEntity;
    public Entity goldHarvesterPrefabEntity;
    public Entity oilHarvesterPrefabEntity;
}
