using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    public event EventHandler OnActiveBuildingDataSOChanged;

    public static BuildingPlacementManager Instance { get; private set; }

    [SerializeField] private BuildingDataSO buildingDataSO;
    [SerializeField] private UnityEngine.Material ghostPrefabMaterial;

    private Transform ghostPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(ghostPrefab != null)
        {
            ghostPrefab.position = MouseWorldPosition.Instance.GetPosition();
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (buildingDataSO.IsNone())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetActiveBuildingDataSO(GameAssets.Instance.buildingDataListSO.none);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (ResourceManager.Instance.CanSpendResourceAmount(buildingDataSO.buildCostResourceAmountArray))
            {
                if (!CanPlaceBuilding())
                {
                    return;
                }

                ResourceManager.Instance.SpendResourceAmount(buildingDataSO.buildCostResourceAmountArray);
                Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<EntitiesReferences>().Build(entityManager);
                EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();

                Entity visualEntity = entityManager.Instantiate(buildingDataSO.GetEntityVisualPrefab(entitiesReferences));
                entityManager.SetComponentData(visualEntity, LocalTransform.FromPosition(mouseWorldPosition + new Vector3(0, buildingDataSO.constructionYOffset, 0)));

                Entity constructionEntity = entityManager.Instantiate(entitiesReferences.buildingConstructionPrefabEntity);
                entityManager.SetComponentData(constructionEntity, LocalTransform.FromPosition(mouseWorldPosition));
                entityManager.SetComponentData(constructionEntity, new BuildingConstruction
                {
                    buildingType = buildingDataSO.buildingType,
                    constructionTimerMax = buildingDataSO.constructionTimerMax,
                    startPosition = mouseWorldPosition + new Vector3(0, buildingDataSO.constructionYOffset, 0),
                    endPosition = mouseWorldPosition,
                    finalPrefabEntity = buildingDataSO.GetEntityPrefab(entitiesReferences),
                    visualEntity = visualEntity
                });
            }
        }
    }

    private bool CanPlaceBuilding()
    {
        Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

        EntityManager entitymanager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entitymanager);
        PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        CollisionFilter collisionFilter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = 1 << GameAssets.BUILDINGS_LAYER,
            GroupIndex = 0,
        };
        
        UnityEngine.BoxCollider boxCollider = buildingDataSO.prefab.GetComponent<UnityEngine.BoxCollider>();
        float bonusExtents = 1.1f;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        if(collisionWorld.OverlapBox(mouseWorldPosition, Quaternion.identity, boxCollider.size * 0.5f * bonusExtents, ref distanceHitList, collisionFilter))
        {
            //Overlaping with other building
            return false;
        }

        distanceHitList.Clear();
        if(collisionWorld.OverlapSphere(mouseWorldPosition, buildingDataSO.buildingDistanceMin, ref distanceHitList, collisionFilter))
        {
            foreach(DistanceHit distanceHit in distanceHitList)
            {
                if (entitymanager.HasComponent<BuildingTypeHolder>(distanceHit.Entity))
                {
                    BuildingType buildingType = entitymanager.GetComponentData<BuildingTypeHolder>(distanceHit.Entity).buildingType;

                    if(buildingType == buildingDataSO.buildingType)
                    {
                        //Same building type is too close
                        return false;
                    }
                }

                if (entitymanager.HasComponent<BuildingConstruction>(distanceHit.Entity))
                {
                    BuildingConstruction buildingConstruction = entitymanager.GetComponentData<BuildingConstruction>(distanceHit.Entity);

                    if (buildingConstruction.buildingType == buildingDataSO.buildingType)
                    {
                        //Same building type is too close
                        return false;
                    }
                }
            }
        }

        if(buildingDataSO is BuildingResourceHarvesterDataSO buildingHarvesterDataSO)
        {
            bool isNearByValidResourceNode = false;

            if (collisionWorld.OverlapSphere(
                mouseWorldPosition,
                buildingHarvesterDataSO.harvestDistanceMin,
                ref distanceHitList,
                new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.RESOURCE_LAYER,
                    GroupIndex = 0
                }))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    if (entitymanager.HasComponent<ResourceTypeHolder>(distanceHit.Entity))
                    {
                        ResourceTypeHolder resourceTypeHolder = entitymanager.GetComponentData<ResourceTypeHolder>(distanceHit.Entity);

                        if (resourceTypeHolder.resourceType == buildingHarvesterDataSO.harvestableResourceType)
                        {
                            //Same building type is too close
                            isNearByValidResourceNode = true;
                        }
                    }
                }
            }

            if(!isNearByValidResourceNode)
            {
                return false;
            }
        }

        return true;
    }

    public BuildingDataSO GetActiveBuildingDataSO()
    {
        return buildingDataSO;
    }

    public void SetActiveBuildingDataSO(BuildingDataSO buildingDataSO)
    {
        this.buildingDataSO = buildingDataSO;

        if(ghostPrefab != null)
        {
            Destroy(ghostPrefab.gameObject);
        }

        if (!buildingDataSO.IsNone())
        {
            ghostPrefab = Instantiate(buildingDataSO.ghostPrefab);

            foreach(MeshRenderer meshRenderer in ghostPrefab.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostPrefabMaterial;
            }
        }

        OnActiveBuildingDataSOChanged?.Invoke(this, EventArgs.Empty);
    }
}