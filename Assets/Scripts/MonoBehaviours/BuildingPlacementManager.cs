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
            if (!CanPlaceBuilding())
            {
                return;
            }

            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<EntitiesReferences>().Build(entityManager);
            EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();

            Entity spawnedEntity = entityManager.Instantiate(buildingDataSO.GetEntityPrefab(entitiesReferences));
            entityManager.SetComponentData(spawnedEntity, LocalTransform.FromPosition(mouseWorldPosition));
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