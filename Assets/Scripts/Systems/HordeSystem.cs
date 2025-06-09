using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct HordeSystem : ISystem
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
        EntityCommandBuffer entityCommanderBuffer = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<Horde> horde)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<Horde>>())
        {
            horde.ValueRW.startTime -= SystemAPI.Time.DeltaTime;

            if (horde.ValueRO.startTime > 0)
            {
                return;
            }

            // Start timer has elapsed

            if (horde.ValueRO.zombieAmountToSpawn <= 0)
            {
                //no zombies to spawn
                return;
            }

            horde.ValueRW.spawnTimer -= SystemAPI.Time.DeltaTime;

            if (horde.ValueRO.spawnTimer <= 0)
            {
                horde.ValueRW.spawnTimer = horde.ValueRO.spawnTimerMax;

                Entity zombieEntity = entityCommanderBuffer.Instantiate(entitiesReferences.zombiePrefabEntity);

                float3 spawnPosition = localTransform.ValueRO.Position;
                Random random = horde.ValueRO.random;

                spawnPosition.x += random.NextFloat(-horde.ValueRO.spawnAreaWidth, +horde.ValueRO.spawnAreaWidth);
                spawnPosition.z += random.NextFloat(-horde.ValueRO.spawnAreaHeight, +horde.ValueRO.spawnAreaHeight);

                horde.ValueRW.random = random;

                entityCommanderBuffer.SetComponent(zombieEntity, LocalTransform.FromPosition(spawnPosition));
                entityCommanderBuffer.AddComponent<EnemyAttackOnHQ>(zombieEntity);

                horde.ValueRW.zombieAmountToSpawn--;
            }
        }
    }
}
