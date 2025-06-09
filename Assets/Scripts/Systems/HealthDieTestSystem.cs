using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthDieTestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        NativeList<Entity> entityList = new NativeList<Entity>(Allocator.Temp);

        foreach((RefRW<Health> health, Entity entity) in SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
        {
            if(health.ValueRO.healthAmount <= 0)
            {
                health.ValueRW.onDead = true;
                entityList.Add(entity);

                if (SystemAPI.HasComponent<BuildingConstruction>(entity))
                {
                    BuildingConstruction buildingConstruction = SystemAPI.GetComponent<BuildingConstruction>(entity);
                    entityCommandBuffer.DestroyEntity(buildingConstruction.visualEntity);
                }
            }
        }

        entityCommandBuffer.DestroyEntity(entityList.AsArray());
    }
}
