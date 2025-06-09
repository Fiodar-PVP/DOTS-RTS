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

    [SerializeField] private GameObject buildingBarrackVisualPrefab;
    [SerializeField] private GameObject buildingTowerVisualPrefab;
    [SerializeField] private GameObject buildingIronHarvesterVisualPrefab;
    [SerializeField] private GameObject buildingGoldHarvesterVisualPrefab;
    [SerializeField] private GameObject buildingOilHarvesterVisualPrefab;

    [SerializeField] private GameObject buildingConstructionPrefab;

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

                barrackVisualPrefabEntity = GetEntity(authoring.buildingBarrackVisualPrefab, TransformUsageFlags.Dynamic),
                towerVisualPrefabEntity = GetEntity(authoring.buildingTowerVisualPrefab, TransformUsageFlags.Dynamic),
                ironHarvesterVisualPrefabEntity = GetEntity(authoring.buildingIronHarvesterVisualPrefab, TransformUsageFlags.Dynamic),
                goldHarvesterVisualPrefabEntity = GetEntity(authoring.buildingGoldHarvesterVisualPrefab, TransformUsageFlags.Dynamic),
                oilHarvesterVisualPrefabEntity = GetEntity(authoring.buildingOilHarvesterVisualPrefab, TransformUsageFlags.Dynamic),

                buildingConstructionPrefabEntity = GetEntity(authoring.buildingConstructionPrefab, TransformUsageFlags.Dynamic)
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

    public Entity barrackVisualPrefabEntity;
    public Entity towerVisualPrefabEntity;
    public Entity ironHarvesterVisualPrefabEntity;
    public Entity goldHarvesterVisualPrefabEntity;
    public Entity oilHarvesterVisualPrefabEntity;

    public Entity buildingConstructionPrefabEntity;
}
