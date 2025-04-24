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

            LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            ShootVictim tragetShootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
            float3 targetShootPosition = targetTransform.TransformPoint(tragetShootVictim.hitPositionLocal);

            float3 moveDirection = targetShootPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            float distanceBefore = math.distancesq(targetShootPosition, localTransform.ValueRO.Position);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distanceAfter = math.distancesq(targetShootPosition, localTransform.ValueRO.Position);

            if(distanceAfter > distanceBefore)
            {
                //Overshoot the target
                localTransform.ValueRW.Position = targetShootPosition;
            }

            float destroyDistanceSq = 0.2f;
            float currentDistanceToTargetSq = math.distancesq(targetShootPosition, localTransform.ValueRO.Position);

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
