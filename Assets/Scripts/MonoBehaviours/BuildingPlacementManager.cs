using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    [SerializeField] private BuildingDataSO buildingDataSO;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
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
}