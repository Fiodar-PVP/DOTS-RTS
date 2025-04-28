using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct LoseTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
     foreach((RefRO<LocalTransform> localTransform, RefRO<LoseTarget> loseTarget, RefRW<Target> target) in 
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<LoseTarget>, RefRW<Target>>())
        {
            if(target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            LocalTransform targetLocatTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float targetDistanceSq = math.distancesq(localTransform.ValueRO.Position, targetLocatTransform.Position);
            if (targetDistanceSq > loseTarget.ValueRO.loseTargetDistanceSq)
            {
                //Target is too far, reset it
                target.ValueRW.targetEntity = Entity.Null;
            }
        }
    }
}
