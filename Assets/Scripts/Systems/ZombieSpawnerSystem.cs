using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<ZombieSpawner> zombieSpawner) in 
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
        {
            zombieSpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if(zombieSpawner.ValueRO.timer > 0)
            {
                //Timer has not elapsed yet
                continue;
            }

            zombieSpawner.ValueRW.timer = zombieSpawner.ValueRO.timerMax;

            distanceHitList.Clear();

            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << GameAssets.UNIT_LAYER,
                GroupIndex = 0,
            };

            int nearByZombieAmount = 0;
            if (collisionWorld.OverlapSphere(
                localTransform.ValueRO.Position, 
                zombieSpawner.ValueRO.nearbyZombieDistanceMax, 
                ref distanceHitList, 
                collisionFilter))
            {
                foreach(DistanceHit distanceHit in distanceHitList)
                {
                    if (!SystemAPI.Exists(distanceHit.Entity))
                    {
                        continue;
                    }

                    if(SystemAPI.HasComponent<Unit>(distanceHit.Entity) && SystemAPI.HasComponent<Faction>(distanceHit.Entity))
                    {
                        Faction faction = SystemAPI.GetComponent<Faction>(distanceHit.Entity);
                        if (faction.factionType == FactionType.Zombie)
                        {
                            nearByZombieAmount++;
                        }
                    }
                }
            }

            if(nearByZombieAmount >= zombieSpawner.ValueRO.nearbyZombieAmountMax)
            {
                continue;
            }

            Entity zombieEntity = state.EntityManager.Instantiate(entitiesReferences.zombiePrefabEntity);
            SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            entityCommandBuffer.AddComponent(zombieEntity, new RandomWalking
            {
                targetPosition = localTransform.ValueRO.Position,
                originPosition = localTransform.ValueRO.Position,
                distanceMin = zombieSpawner.ValueRO.randomWalkingDistanceMin,
                distanceMax = zombieSpawner.ValueRO.randomWalkingDistanceMax,
                random = new Random((uint)zombieEntity.Index)
            });
        }
    }
}
