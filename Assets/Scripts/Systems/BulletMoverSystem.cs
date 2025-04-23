using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeList<Entity> entityList = new NativeList<Entity>(Allocator.Temp);

        foreach((RefRW<LocalTransform> localTransform, RefRO<Bullet> bullet, RefRO<Target> target, Entity entity) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                entityList.Add(entity);
                continue;
            }

            RefRO<LocalTransform> targetTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);
            float3 moveDirection = targetTransform.ValueRO.Position - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            float distanceBefore = math.distancesq(targetTransform.ValueRO.Position, localTransform.ValueRO.Position);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distanceAfter = math.distancesq(targetTransform.ValueRO.Position, localTransform.ValueRO.Position);

            if(distanceAfter > distanceBefore)
            {
                //Overshoot the target
                localTransform.ValueRW.Position = targetTransform.ValueRO.Position;
            }

            float destroyDistanceSq = 0.2f;
            float currentDistanceToTargetSq = math.distancesq(targetTransform.ValueRO.Position, localTransform.ValueRO.Position);

            if(currentDistanceToTargetSq <= destroyDistanceSq)
            {
                entityList.Add(entity);

                RefRW<Health> health = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                health.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
            }
        }

        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        entityCommandBuffer.DestroyEntity(entityList.AsArray());
    }
}
